Public Class Logger
    Implements ILogger

    Public Function GetDefaultLogger() As ILogger Implements ILogger.GetDefaultLogger
        Return New Logger
    End Function

    Public Sub Write(loggerMessageLevel As LoggerMessageLevel, smsXMessageIdToSendByAts As String, id As Integer, target As String, resultMessage As String) Implements ILogger.Write
        ' TODO: Implement
    End Sub

    Public Sub Write(loggerMessageLevel As LoggerMessageLevel, smsXMessageIdToSendByAts As String, id As Integer, target As String) Implements ILogger.Write
        ' TODO: Implement
    End Sub

End Class

Public Enum LoggerMessageLevel

    Info
    [Error]

End Enum