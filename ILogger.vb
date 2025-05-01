Public Interface ILogger

    Function GetDefaultLogger() As ILogger

    Sub Write(loggerMessageLevel As LoggerMessageLevel, smsXMessageIdToSendByAts As String, id As Integer, target As String, resultMessage As String)

    Sub Write(loggerMessageLevel As LoggerMessageLevel, smsXMessageIdToSendByAts As String, id As Integer, target As String)

End Interface
