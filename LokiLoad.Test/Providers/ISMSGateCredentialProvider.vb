Public Interface ISMSGateCredentialProvider

    Function GetSMSGateCredentials(message As Message, useDaktela As Boolean) As SMSGateCredentials

End Interface
