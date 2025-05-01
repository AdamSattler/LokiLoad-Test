Imports LokiLoad.Test.GSMConnector.GSM.Models

Public Class MessageManager

    Private Shared Property ATSLogin As String = System.Configuration.ConfigurationManager.AppSettings("ATSLogin")

    Private Shared Property ATSPassword As String = System.Configuration.ConfigurationManager.AppSettings("ATSPassword")

    Private Shared Property DaktelaLogin As String = System.Configuration.ConfigurationManager.AppSettings("DaktelaLogin")

    Private Shared Property DaktelaPassword As String = System.Configuration.ConfigurationManager.AppSettings("DaktelaPassword")

    Public Shared Property UseDaktela As Boolean

    Private ReadOnly _logger As ILogger

    Private ReadOnly _gsmServiceClientFactory As IGSMServiceClientFactory

    Public Sub New(logger As ILogger, gsmServiceClientFactory As IGSMServiceClientFactory)

        _logger = logger
        _gsmServiceClientFactory = gsmServiceClientFactory

    End Sub

    Public Sub SendSMSMessage(ByVal myMessage As Message)
        Dim debugMode As Boolean = False
        Dim debugTo() As String = {}

        If System.Configuration.ConfigurationManager.AppSettings("DebugMode") IsNot Nothing AndAlso System.Configuration.ConfigurationManager.AppSettings("DebugMode").ToLowerInvariant = "true" AndAlso System.Configuration.ConfigurationManager.AppSettings("Messages.GSM.Debug.To") IsNot Nothing AndAlso Not String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings("Messages.GSM.Debug.To")) Then
            debugMode = True
            debugTo = System.Configuration.ConfigurationManager.AppSettings("Messages.GSM.Debug.To").Replace(",", ";").Split(";")
        End If

        Dim finalTarget As New List(Of String)

        If debugMode Then

            For Each t As String In debugTo
                finalTarget.Add(t)
            Next

        Else
            finalTarget.Add(myMessage.Target)

            If Not String.IsNullOrEmpty(myMessage.TargetCC) Then
                finalTarget.Add(myMessage.TargetCC)
            End If

            If Not String.IsNullOrEmpty(myMessage.TargetBCC) Then
                finalTarget.Add(myMessage.TargetBCC)
            End If

        End If


        'INFO: Odesilam podle SMSGate
        If myMessage.SmsGate.HasValue AndAlso myMessage.SmsGate.Value = MessageSMSGate.ATS Then
            'INFO: Výběr GSM Brány
            Dim ATSGSMLogin As String = ATSLogin
            Dim ATSGSMPassword As String = ATSPassword

            Using service As IGSMServiceClient = _gsmServiceClientFactory.CreateClient()

                For Each target As String In finalTarget

                    Try
                        myMessage.DateOfSend = Now
                        Dim resSendSMS As GSMConnector.GSM.Models.WSMessageSendResponse = service.SendSMS(ATSGSMLogin, ATSGSMPassword, target, myMessage.Body, "", "", "", True, Nothing, Nothing)
                        If resSendSMS.Result Then

                            myMessage.ExternalId = resSendSMS.IdMessage

                            myMessage.Status = MessageStatus.Sending
                            MessageManager.Save(myMessage, False)
                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SMS X-Message-ID={0} to {1} send by ATS.", myMessage.Id, target)

                        Else
                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)

                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS X-Message-ID={0} by ATS to {1}, {2}", myMessage.Id, target, resSendSMS.ResultMessage)
                        End If


                    Catch ex As Exception
                        If ex.ToString.Contains("Nepodarilo se odeslat SMS Ex: Telefon nema nutnych 9 znaku") Then
                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "Message Id={0} to {1} - " + ex.ToString, myMessage.Id, target)

                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)
                        Else
                            Throw ex
                        End If
                    End Try


                Next

            End Using
        ElseIf myMessage.SmsGate.HasValue AndAlso myMessage.SmsGate.Value = MessageSMSGate.Daktela Then
            'INFO: Výběr GSM Brány
            Dim DaktelaGSMLogin As String = DaktelaLogin
            Dim DaktelaGSMPassword As String = DaktelaPassword

            Using service As IGSMServiceClient = _gsmServiceClientFactory.CreateClient()

                For Each target As String In finalTarget

                    Try
                        myMessage.DateOfSend = Now
                        Dim resSendSMS As GSMConnector.GSM.Models.WSMessageSendResponse = service.SendSMS(DaktelaGSMLogin, DaktelaGSMPassword, target, myMessage.Body, "", "", "", True, Nothing, Nothing)
                        If resSendSMS.Result Then

                            myMessage.ExternalId = resSendSMS.IdMessage

                            myMessage.Status = MessageStatus.Sending
                            MessageManager.Save(myMessage, False)
                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SMS X-Message-ID={0} to {1} send by ATS.", myMessage.Id, target)

                        Else
                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)

                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS X-Message-ID={0} by ATS to {1}, {2}", myMessage.Id, target, resSendSMS.ResultMessage)
                        End If


                    Catch ex As Exception
                        If ex.ToString.Contains("Nepodarilo se odeslat SMS Ex: Telefon nema nutnych 9 znaku") Then
                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "Message Id={0} to {1} - " + ex.ToString, myMessage.Id, target)

                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)
                        Else
                            Throw ex
                        End If
                    End Try


                Next

            End Using
        Else
            'INFO: Výběr GSM Brány
            Dim GSMLogin As String
            Dim GSMPassword As String

            If myMessage.Kind.HasValue AndAlso myMessage.Kind.Value = MessageKind.SMSCampaign Then
                Dim simSetting As SimSettings = SimSettingsManager.GetSimSettingForMessage(myMessage.Environment, SimSettingsType.Campaigne)
                If simSetting IsNot Nothing Then
                    myMessage.GatewayId = simSetting.GatewayId
                    GSMLogin = simSetting.Login
                    GSMPassword = simSetting.Password
                Else

                    _logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS - Not more SIM Campaigne space. Message-ID={0} to {1}", myMessage.Id, myMessage.Target)
                    Throw New Exception("Error sending SMS - Not more SIM Campaigne space.")
                End If

            Else
                Dim simSetting As SimSettings = SimSettingsManager.GetSimSettingForMessage(myMessage.Environment, SimSettingsType.Standard)
                If simSetting IsNot Nothing Then
                    myMessage.GatewayId = simSetting.GatewayId
                    GSMLogin = simSetting.Login
                    GSMPassword = simSetting.Password
                Else
                    If UseDaktela Then
                        GSMLogin = DaktelaLogin
                        GSMPassword = DaktelaPassword
                        myMessage.GatewayId = 100
                    Else
                        _logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS - Not more SIM Standard space. Message-ID={0} to {1}", myMessage.Id, myMessage.Target)
                        Throw New Exception("Error sending SMS - Not more SIM Standard space.")
                    End If
                End If

            End If

            Using service As IGSMServiceClient = _gsmServiceClientFactory.CreateClient()

                For Each target As String In finalTarget

                    Try
                        myMessage.DateOfSend = Now
                        Dim resSendSMS As GSMConnector.GSM.Models.WSMessageSendResponse = service.SendSMS(GSMLogin, GSMPassword, target, myMessage.Body, "", "", "", True, Nothing, Nothing)
                        If resSendSMS.Result Then

                            myMessage.ExternalId = resSendSMS.IdMessage

                            myMessage.Status = MessageStatus.Sending
                            MessageManager.Save(myMessage, False)
                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SMS X-Message-ID={0} to {1} send.", myMessage.Id, target)

                        Else
                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)

                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS X-Message-ID={0} to {1}, {2}", myMessage.Id, target, resSendSMS.ResultMessage)
                        End If


                    Catch ex As Exception
                        If ex.ToString.Contains("Nepodarilo se odeslat SMS Ex: Telefon nema nutnych 9 znaku") Then
                            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "Message Id={0} to {1} - " + ex.ToString, myMessage.Id, target)

                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)
                        Else
                            Throw ex
                        End If
                    End Try


                Next

            End Using
        End If


    End Sub

    Private Shared Function Save(myMessage As Message, b As Boolean) As WSMessageSendResponse
        ' TODO: Implement
    End Function

End Class
