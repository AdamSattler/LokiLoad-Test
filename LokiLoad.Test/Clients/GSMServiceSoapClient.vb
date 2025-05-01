
Imports LokiLoad.Test.GSMConnector.GSM.Models

Namespace GSMConnector.GSM

    Public Class GSMServiceSoapClient
        Implements IGSMServiceClient

        Public Function SendSMS(daktelaGsmLogin As String, daktelaGsmPassword As String, target As String, body As String, empty As String, s As String, empty1 As String, b As Boolean, o As Object, o1 As Object) As Models.WSMessageSendResponse Implements IGSMServiceClient.SendSMS
            Throw New NotImplementedException
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose

        End Sub

        Private Function IGSMServiceSoapClient_SendSMS(daktelaGsmLogin As String, daktelaGsmPassword As String, target As String, body As String, empty As String, s As String, empty1 As String, b As Boolean, o As Object, o1 As Object) As WSMessageSendResponse
            Throw New NotImplementedException()
        End Function
    End Class

End Namespace
