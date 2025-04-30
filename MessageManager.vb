Imports LokiLoad.Test.GSMConnector.GSM

Public Class MessageManager
    Private Shared Property ATSLogin As String = System.Configuration.ConfigurationManager.AppSettings("ATSLogin")

    Private Shared Property ATSPassword As String = System.Configuration.ConfigurationManager.AppSettings("ATSPassword")
    
    Private Shared Property DaktelaLogin As String = System.Configuration.ConfigurationManager.AppSettings("DaktelaLogin")
    
    Private Shared Property DaktelaPassword As String = System.Configuration.ConfigurationManager.AppSettings("DaktelaPassword")
    
    Public Shared Property UseDaktela As Boolean
    
    Private Shared Sub SendSMSMessage(ByVal myMessage As Message)
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
        If myMessage.SmsGate.HasValue AndAlso myMessage.SmsGate.Value = MessageSmsGate.ATS Then
            'INFO: Výběr GSM Brány
            Dim ATSGSMLogin As String = ATSLogin
            Dim ATSGSMPassword As String = ATSPassword

            Using service As New GSMConnector.GSM.GSMServiceSoapClient ' GSMConnector.GSM.GMSServiceSoapClient()

                For Each target As String In finalTarget

                    Try
                        myMessage.DateOfSend = Now
                        Dim resSendSMS As GSMConnector.GSM.WSMessageSendResponse = service.SendSMS(ATSGSMLogin, ATSGSMPassword, target, myMessage.Body, "", "", "", True, Nothing, Nothing)
                        If resSendSMS.Result Then

                            myMessage.ExternalId = resSendSMS.IdMessage

                            myMessage.Status = MessageStatus.Sending
                            MessageManager.Save(myMessage, False)
                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SMS X-Message-ID={0} to {1} send by ATS.", myMessage.Id, target)

                        Else
                            'Log("Zprava nebyla odeslana Result: " + resSendSMS.ResultMessage)
                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)

                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS X-Message-ID={0} by ATS to {1}, {2}", myMessage.Id, target, resSendSMS.ResultMessage)
                        End If


                    Catch ex As Exception
                        If ex.ToString.Contains("Nepodarilo se odeslat SMS Ex: Telefon nema nutnych 9 znaku") Then
                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "Message Id={0} to {1} - " + ex.ToString, myMessage.Id, target)

                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)
                        Else
                            Throw ex
                        End If
                    End Try


                Next

            End Using
        ElseIf myMessage.SmsGate.HasValue AndAlso myMessage.SmsGate.Value = MessageSmsGate.Daktela Then
            'INFO: Výběr GSM Brány
            Dim DaktelaGSMLogin As String = DaktelaLogin
            Dim DaktelaGSMPassword As String = DaktelaPassword

            Using service As New GSMConnector.GSM.GSMServiceSoapClient ' GSMConnector.GSM.GMSServiceSoapClient()

                For Each target As String In finalTarget

                    Try
                        myMessage.DateOfSend = Now
                        Dim resSendSMS As GSMConnector.GSM.WSMessageSendResponse = service.SendSMS(DaktelaGSMLogin, DaktelaGSMPassword, target, myMessage.Body, "", "", "", True, Nothing, Nothing)
                        If resSendSMS.Result Then

                            myMessage.ExternalId = resSendSMS.IdMessage

                            myMessage.Status = MessageStatus.Sending
                            MessageManager.Save(myMessage, False)
                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SMS X-Message-ID={0} to {1} send by ATS.", myMessage.Id, target)

                        Else
                            'Log("Zprava nebyla odeslana Result: " + resSendSMS.ResultMessage)
                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)

                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS X-Message-ID={0} by ATS to {1}, {2}", myMessage.Id, target, resSendSMS.ResultMessage)
                        End If


                    Catch ex As Exception
                        If ex.ToString.Contains("Nepodarilo se odeslat SMS Ex: Telefon nema nutnych 9 znaku") Then
                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "Message Id={0} to {1} - " + ex.ToString, myMessage.Id, target)

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

                    Logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS - Not more SIM Campaigne space. Message-ID={0} to {1}", myMessage.Id, myMessage.Target)
                    Throw New Exception("Error sending SMS - Not more SIM Campaigne space.")
                End If

                'If MaxCampaignSMS < TodaySendSMSCampaign(myMessage.Environment) Then
                '    Exit Sub
                'End If
                'Dim gateWay As Integer = GetLastUsedGatewayCampaign(myMessage.Environment) + 1 - 100
                'If gateWay > NumberOfCampaignGateway OrElse gateWay = -99 Then gateWay = 1
                'myMessage.GatewayId = gateWay + 100
                'GSMLogin = GetSMSLoginForGatewayCampaign(gateWay)
                'GSMPassword = GetSMSPasswordForGatewayCampaign(gateWay)
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
                        Logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS - Not more SIM Standard space. Message-ID={0} to {1}", myMessage.Id, myMessage.Target)
                        Throw New Exception("Error sending SMS - Not more SIM Standard space.")
                    End If
                End If


                'Dim daktela As Boolean = False
                'If UseDaktela AndAlso MaxStandardSMS.HasValue Then
                '    If MaxStandardSMS < TodaySendSMS(myMessage.Environment) Then
                '        daktela = True
                '    End If
                'End If
                'If daktela Then
                '    GSMLogin = DaktelaLogin
                '    GSMPassword = DaktelaPassword
                '    myMessage.GatewayId = 100
                'Else


                '    SimSettingsManager.
                '    Dim gateWay As Integer = GetLastUsedGateway(myMessage.Environment) + 1
                '    ' Logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SendSMSMessage - GateWay1=" + gateWay.ToString + ", NumberOfStandardGateway=" + NumberOfStandardGateway.ToString)
                '    If gateWay > NumberOfStandardGateway Then gateWay = 1
                '    ' Logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SendSMSMessage - GateWayFinal=" + gateWay.ToString)
                '    myMessage.GatewayId = gateWay
                '    GSMLogin = GetSMSLoginForGateway(gateWay)
                '    GSMPassword = GetSMSPasswordForGateway(gateWay)
                'End If
            End If



            Using service As New GSMConnector.GSM.GSMServiceSoapClient ' GSMConnector.GSM.GMSServiceSoapClient()

                For Each target As String In finalTarget

                    Try
                        myMessage.DateOfSend = Now
                        Dim resSendSMS As GSMConnector.GSM.WSMessageSendResponse = service.SendSMS(GSMLogin, GSMPassword, target, myMessage.Body, "", "", "", True, Nothing, Nothing)
                        If resSendSMS.Result Then

                            myMessage.ExternalId = resSendSMS.IdMessage

                            myMessage.Status = MessageStatus.Sending
                            MessageManager.Save(myMessage, False)
                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SMS X-Message-ID={0} to {1} send.", myMessage.Id, target)

                        Else
                            'Log("Zprava nebyla odeslana Result: " + resSendSMS.ResultMessage)
                            myMessage.Status = MessageStatus.Error
                            MessageManager.Save(myMessage, False)

                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS X-Message-ID={0} to {1}, {2}", myMessage.Id, target, resSendSMS.ResultMessage)
                        End If


                    Catch ex As Exception
                        If ex.ToString.Contains("Nepodarilo se odeslat SMS Ex: Telefon nema nutnych 9 znaku") Then
                            Logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "Message Id={0} to {1} - " + ex.ToString, myMessage.Id, target)

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
        Throw New NotImplementedException
    End Function
End Class
