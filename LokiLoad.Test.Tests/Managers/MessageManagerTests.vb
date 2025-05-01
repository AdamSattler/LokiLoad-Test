Imports Moq

<TestClass()>
Public Class MessageManagerTests
    Private gsmServiceClientFactoryMock As Mock(Of IGSMServiceClientFactory)
    Private loggerMock As Mock(Of ILogger)
    Private gsmServiceClientMock As Mock(Of IGSMServiceClient)
    Private smsGateCredentialProvider As Mock(Of ISMSGateCredentialProvider)

    <TestInitialize()>
    Public Sub TestInitialize()
        gsmServiceClientFactoryMock = New Mock(Of IGSMServiceClientFactory)
        loggerMock = New Mock(Of ILogger)
        gsmServiceClientMock = New Mock(Of IGSMServiceClient)
        smsGateCredentialProvider = New Mock(Of ISMSGateCredentialProvider)
        MessageManager.UseDaktela = False
    End Sub


    <TestMethod()>
    Public Sub SendSMSMessage_WithDebugMode_ShouldSendToDebugRecipients()
        ' Arrange
        System.Configuration.ConfigurationManager.AppSettings(Consts.Settings.DebugMode) = "true"
        System.Configuration.ConfigurationManager.AppSettings(Consts.Settings.MessagesDebugTo) = "123456789,987654321"

        Dim message = New Message With {
            .SmsGate = MessageSMSGate.ATS,
            .Target = "555123456",
            .Body = "Test message",
            .Status = MessageStatus.Sending
        }

        Dim successResponse = New GSMConnector.GSM.Models.WSMessageSendResponse With {
            .Result = True,
            .IdMessage = "MSG123",
            .ResultMessage = "Success"
        }

        Dim credentials = New SMSGateCredentials("login", "password")

        gsmServiceClientMock.Setup(Function(s) s.SendSMS(It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of Boolean), Nothing, Nothing)).Returns(successResponse)
        gsmServiceClientFactoryMock.Setup(Function(s) s.CreateClient()).Returns(gsmServiceClientMock.Object)
        loggerMock.Setup(Function(s) s.GetDefaultLogger()).Returns(loggerMock.Object)
        smsGateCredentialProvider.Setup(Function(s) s.GetSMSGateCredentials(It.IsAny(Of Message), It.IsAny(Of Boolean))).Returns(credentials)

        ' Act
        Dim messageManager = New MessageManager(loggerMock.Object, gsmServiceClientFactoryMock.Object, smsGateCredentialProvider.Object)
        messageManager.SendSMSMessage(message)

        ' Assert
        gsmServiceClientMock.Verify(Function(s) s.SendSMS(It.IsAny(Of String), It.IsAny(Of String), "123456789", It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of Boolean), Nothing, Nothing), Times.Once)
        gsmServiceClientMock.Verify(Function(s) s.SendSMS(It.IsAny(Of String), It.IsAny(Of String), "987654321", It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of Boolean), Nothing, Nothing), Times.Once)
        Assert.AreEqual(MessageStatus.Sending, message.Status)
        Assert.AreEqual("MSG123", message.ExternalId)
    End Sub

End Class
