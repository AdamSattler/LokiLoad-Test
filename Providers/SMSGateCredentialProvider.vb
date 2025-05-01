Imports System.Runtime.Remoting.Messaging

Public Class SMSGateCredentialProvider
    Implements ISMSGateCredentialProvider

    Private ReadOnly _logger As ILogger

    Public Sub New(logger As ILogger)

        _logger = logger

    End Sub

    Public Function GetSMSGateCredentials(message As Message, useDaktela As Boolean) As SMSGateCredentials Implements ISMSGateCredentialProvider.GetSMSGateCredentials

        If message.SmsGate.HasValue Then

            'INFO: Získám přístupové ůdaje podle SMSGate
            Return GetSMSGateCredentialsByMessageSMSGate(message.SmsGate)

        Else

            'INFO: Získám přístupové ůdaje podle druhu zprávy
            Return GetSMSGateCredentialsByKind(message, useDaktela)

        End If

    End Function

    Private Function GetSMSGateCredentialsByMessageSMSGate(smsGateType As MessageSMSGate) As SMSGateCredentials

        Select Case smsGateType
            Case MessageSMSGate.ATS
                Return New SMSGateCredentialATS
            Case MessageSMSGate.Daktela
                Return New SMSGateCredentialDaktela
        End Select

        Throw New NotImplementedException

    End Function

    Private Function GetSMSGateCredentialsByKind(message As Message, useDaktela As Boolean) As SMSGateCredentials

        If message.Kind.HasValue AndAlso message.Kind.Value = MessageKind.SMSCampaign Then
            Dim simSettings = SimSettingsManager.GetSimSettingForMessage(message.Environment, SimSettingsType.Campaigne)

            If simSettings Is Nothing Then
                _logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS - Not more SIM Campaigne space. Message-ID={0} to {1}", message.Id, message.Target)
                Throw New Exception("Error sending SMS - Not more SIM Campaigne space.")
            End If

            Return MapSimSettingsToSMSGateCredentials(simSettings)
        Else
            Dim simSettings As SimSettings = SimSettingsManager.GetSimSettingForMessage(message.Environment, SimSettingsType.Standard)

            If simSettings IsNot Nothing Then
                Return MapSimSettingsToSMSGateCredentials(simSettings)
            End If

            If useDaktela Then
                Return GetDakatelaCredentialsWithGatewayId()
            End If

            _logger.GetDefaultLogger.Write(LoggerMessageLevel.Error, "Error sending SMS - Not more SIM Standard space. Message-ID={0} to {1}", message.Id, message.Target)
            Throw New Exception("Error sending SMS - Not more SIM Standard space.")
        End If

    End Function

    Private Function MapSimSettingsToSMSGateCredentials(simSettings As SimSettings) As SMSGateCredentials

        Return New SMSGateCredentials(simSettings.Login, simSettings.Password, simSettings.GatewayId)

    End Function

    Private Function GetDakatelaCredentialsWithGatewayId() As SMSGateCredentialDaktela

        Dim daktelaCredentials = New SMSGateCredentialDaktela
        daktelaCredentials.GatewayId = 100

        Return daktelaCredentials

    End Function

End Class
