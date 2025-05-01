Public Class SMSGateCredentials

    Public Property Login As String

    Public Property Password As String

    Public Property GatewayId As Integer

    Public Sub New(login As String, password As String)
        Me.Login = login
        Me.Password = password
    End Sub

    Public Sub New(login As String, password As String, gatewayId As Integer)
        Me.Login = login
        Me.Password = password
        Me.GatewayId = gatewayId
    End Sub

End Class
