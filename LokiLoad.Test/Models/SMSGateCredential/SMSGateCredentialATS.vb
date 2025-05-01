Imports System.Configuration

Public Class SMSGateCredentialATS
    Inherits SMSGateCredentials

    Public Sub New()
        MyBase.New(ConfigurationManager.AppSettings("ATSLogin"), ConfigurationManager.AppSettings("ATSPassword"))
    End Sub

End Class
