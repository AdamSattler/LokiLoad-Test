Imports LokiLoad.Test.GSMConnector.GSM.Models
Imports System.Configuration
Imports System.Runtime.Remoting.Messaging

Public Class MessageManager

    Private ReadOnly _logger As ILogger

    Private ReadOnly _gsmServiceClientFactory As IGSMServiceClientFactory

    Private ReadOnly _smsGateCredentialProvider As ISMSGateCredentialProvider

    Public Shared Property UseDaktela As Boolean

    Public Sub New(logger As ILogger, gsmServiceClientFactory As IGSMServiceClientFactory, smsGateCredentialProvider As ISMSGateCredentialProvider)

        _logger = logger
        _gsmServiceClientFactory = gsmServiceClientFactory
        _smsGateCredentialProvider = smsGateCredentialProvider

    End Sub

    Public Sub SendSMSMessage(ByVal message As Message)

        'INFO: Získám seznam příjemců
        Dim recipients = GetRecipients(message)

        'INFO: Získám přihlašovací ůdaje
        Dim credentials = _smsGateCredentialProvider.GetSMSGateCredentials(message, UseDaktela)

        'INFO: Odešleme zprávu
        SendMessageViaGateway(message, recipients, credentials)

    End Sub

#Region "Recipients"
    Private Function GetRecipients(message As Message) As IEnumerable(Of String)

        'INFO: Pokud máme debug mod, získáme příjemce z app settings
        If IsDebugMode() Then
            Return GetDebugRecipients()
        End If

        Dim recipients = New List(Of String) From {message.Target}

        If Not String.IsNullOrEmpty(message.TargetCC) Then
            recipients.Add(message.TargetCC)
        End If

        If Not String.IsNullOrEmpty(message.TargetBCC) Then
            recipients.Add(message.TargetBCC)
        End If

        Return recipients

    End Function

    Private Function IsDebugMode() As Boolean

        Dim debugModeSetting = ConfigurationManager.AppSettings(Consts.Settings.DebugMode)
        Dim debugToSetting = ConfigurationManager.AppSettings(Consts.Settings.MessagesDebugTo)

        Return debugModeSetting IsNot Nothing AndAlso
               debugModeSetting.Equals("true", StringComparison.InvariantCultureIgnoreCase) AndAlso
               debugToSetting IsNot Nothing AndAlso
               Not String.IsNullOrEmpty(debugToSetting)

    End Function

    Private Function GetDebugRecipients() As IEnumerable(Of String)

        Dim debugTo = ConfigurationManager.AppSettings(Consts.Settings.MessagesDebugTo)
        Return debugTo.Replace(",", ";").Split(";")

    End Function
#End Region

#Region "Send Message"
    Private Sub SendMessageViaGateway(message As Message, recipients As IEnumerable(Of String), credentials As SMSGateCredentials)

        'INFO: Vytvoříme nový disposable GSMServiceClient
        Using client = _gsmServiceClientFactory.CreateClient()

            'INFO: Odesíláme zprávy jednotlivým příjemcům
            For Each target In recipients
                Try
                    SendSingleMessage(client, message, target, credentials)
                    _logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "SMS X-Message-ID={0} to {1} send.", message.Id, target)
                Catch ex As Exception
                    HandleSendError(ex, message, target)
                End Try
            Next
        End Using

    End Sub

    Private Sub SendSingleMessage(client As IGSMServiceClient, message As Message,
                                target As String, credentials As SMSGateCredentials)
        message.DateOfSend = DateTime.Now
        Dim response = client.SendSMS(credentials.Login, credentials.Password, target, message.Body, "", "", "", True, Nothing, Nothing)

        If response.Result Then
            message.ExternalId = response.IdMessage
            message.Status = MessageStatus.Sending
            MessageManager.Save(message, False)
        Else
            message.Status = MessageStatus.Error
            MessageManager.Save(message, False)
            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS X-Message-ID={0} by ATS to {1}, {2}", message.Id, target, response.ResultMessage)
        End If
    End Sub
#End Region

#Region "Error Handle"
    Private Sub HandleSendError(ex As Exception, message As Message, target As String)
        If ex.ToString().Contains("Nepodarilo se odeslat SMS Ex: Telefon nema nutnych 9 znaku") Then
            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Info, "Message Id={0} to {1} - " + ex.ToString, message.Id, target)
            message.Status = MessageStatus.Error
            MessageManager.Save(message, False)
        Else
            Throw ex
        End If
    End Sub
#End Region

    Private Shared Function Save(myMessage As Message, b As Boolean) As WSMessageSendResponse
        ' TODO: Implement
    End Function

End Class
