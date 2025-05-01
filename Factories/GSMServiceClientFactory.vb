Imports LokiLoad.Test.GSMConnector.GSM

Public Class GSMServiceClientFactory
    Implements IGSMServiceClientFactory

    Public Function CreateClient() As IGSMServiceClient Implements IGSMServiceClientFactory.CreateClient

        Return New GSMServiceSoapClient

    End Function

End Class
