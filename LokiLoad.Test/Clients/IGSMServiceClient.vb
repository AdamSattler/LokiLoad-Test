Imports LokiLoad.Test.GSMConnector.GSM.Models

Public Interface IGSMServiceClient
    Inherits IDisposable

    Function SendSMS(daktelaGsmLogin As String, daktelaGsmPassword As String, target As String, body As String, empty As String, s As String, empty1 As String, b As Boolean, o As Object, o1 As Object) As WSMessageSendResponse

End Interface
