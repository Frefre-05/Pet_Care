from enum import IntFlag

import comtypes.gen._C866CA3A_32F7_11D2_9602_00C04F8EE628_0_5_4 as __wrapper_module__
from comtypes.gen._C866CA3A_32F7_11D2_9602_00C04F8EE628_0_5_4 import (
    SITooFast, DISPID_SAFSetWaveFormatEx, SPSHORTCUTPAIRLIST,
    SGDSActive, SPINTERFERENCE_TOOQUIET, ISpeechCustomStream,
    Speech_Default_Weight, SRERecognition, SAFTCCITT_uLaw_11kHzMono,
    ISpeechMemoryStream, DISPID_SVVolume, SGDSActiveUserDelimited,
    eLEXTYPE_USER, SECFIgnoreCase, SAFT11kHz16BitStereo,
    DISPID_SAStatus, SGRSTTDictation, DISPID_SPEDisplayAttributes,
    SPDKL_DefaultLocation, DISPID_SRCEHypothesis, tagSPPROPERTYINFO,
    DISPID_SRGCmdLoadFromFile, SDA_Two_Trailing_Spaces, ISpProperties,
    SPEI_TTS_BOOKMARK, SASPause, SPVPRI_NORMAL,
    DISPID_SGRsCommitAndSave, SPPHRASEPROPERTY, ISpeechMMSysAudio,
    SWTDeleted, SAFTADPCM_8kHzMono, SGLexicalNoSpecialChars,
    SPAR_Medium, ISpeechAudioFormat, DISPID_SGRSTs_NewEnum,
    SPRST_NUM_STATES, SPRS_INACTIVE, SDTAlternates,
    DISPID_SPEAudioStreamOffset, DISPID_SPIReplacements,
    DISPID_SPEDisplayText, SPAUDIOBUFFERINFO,
    ISpeechGrammarRuleStateTransitions, SPEI_SR_BOOKMARK,
    SREStateChange, DISPID_SRRPhraseInfo,
    SPWT_LEXICAL_NO_SPECIAL_CHARS, DISPID_SPRFirstElement, SVPNormal,
    DISPID_SPARecoResult, DISPID_SVPriority,
    DISPIDSPTSI_SelectionOffset, DISPID_SGRSTransitions,
    DISPID_SVSInputWordPosition, SpeechTokenValueCLSID,
    SpeechRegistryUserRoot, SPPS_Unknown, SGDSActiveWithAutoPause,
    SPEI_TTS_AUDIO_LEVEL, tagSTATSTG, SVP_1,
    SSSPTRelativeToCurrentPosition, SPBO_TIME_UNITS, ISpVoice,
    DISPID_SLGenerationId, DISPID_SPPParent, IEnumString,
    SPLO_DYNAMIC, SVSFParseSsml, eLEXTYPE_PRIVATE3, SGRSTTWord,
    eLEXTYPE_MORPHOLOGY, DISPID_SPRNumberOfElements,
    DISPID_SDKCreateKey, SPPS_Verb, DISPID_SRGCmdLoadFromResource,
    SVP_6, SREPropertyNumChange, SPPS_RESERVED1, SAFT48kHz16BitMono,
    SPWT_PRONUNCIATION, eLEXTYPE_PRIVATE15, DISPID_SVSVisemeId,
    STSF_FlagCreate, SpeechPropertyNormalConfidenceThreshold,
    SAFTCCITT_uLaw_44kHzStereo, DISPID_SASState, SP_VISEME_15,
    ISpeechObjectTokens, DISPID_SFSClose, eLEXTYPE_PRIVATE4,
    SDKLDefaultLocation, DISPID_SOTDisplayUI, SP_VISEME_4,
    ISpPhraseAlt, DISPID_SPRsCount, DISPID_SWFEExtraData,
    SPEI_RESERVED2, SPPS_RESERVED3, DISPID_SOTCSetId,
    eLEXTYPE_VENDORLEXICON, DISPID_SPEAudioSizeTime, SVP_12,
    SpeechPropertyLowConfidenceThreshold, SP_VISEME_2,
    ISpeechGrammarRule, ISpeechObjectToken, DISPID_SABIEventBias,
    SPWORDLIST, SAFT11kHz8BitMono, SAFTText, eLEXTYPE_RESERVED9,
    ISpObjectTokenCategory, DISPID_SVEventInterests, ISpShortcut,
    SPEI_SR_RETAINEDAUDIO, DISPID_SVSpeakStream, SVEEndInputStream,
    DISPID_SRCERecognition, SPEI_RESERVED5, LONG_PTR,
    DISPID_SWFEChannels, SPINTERFERENCE_TOOSLOW, ISpeechFileStream,
    DISPID_SPEsCount, SBOPause, SVP_21, DISPID_SRSClsidEngine,
    DISPID_SASFreeBufferSpace, ISpNotifySource, DISPID_SDKDeleteValue,
    ISpRecoGrammar, DISPID_SGRSTRule, SAFT11kHz16BitMono,
    SREHypothesis, ISpPhoneConverter, SP_VISEME_13,
    SPSEMANTICERRORINFO, DISPID_SVGetProfiles, SPDKL_CurrentConfig,
    SPSFunction, SPWT_LEXICAL, DISPID_SRGState, DISPID_SOTRemove,
    SPAS_RUN, DISPID_SRStatus, DISPID_SPEAudioSizeBytes,
    ISpeechPhraseReplacements, DISPID_SASNonBlockingIO,
    DISPID_SVVoice, SGLexical, DISPID_SGRAddResource,
    ISpeechBaseStream, SpInProcRecoContext, SPRECOCONTEXTSTATUS,
    SDTProperty, ISpeechPhraseRules, SDTAll, SPXRO_Alternates_SML,
    SPSMF_SRGS_SEMANTICINTERPRETATION_MS, DISPID_SDKGetlongValue,
    SPEI_SR_PRIVATE, DISPID_SLAddPronunciation,
    SpeechRecoProfileProperties, DISPID_SPRDisplayAttributes,
    SPEI_TTS_PRIVATE, SpeechGrammarTagDictation, DISPID_SRGRules,
    SGDSInactive, SSTTTextBuffer, SAFT8kHz16BitStereo,
    SDA_No_Trailing_Space, SPPS_Modifier, ISpLexicon, SVSFUnusedFlags,
    SAFTCCITT_uLaw_22kHzStereo, SAFTCCITT_ALaw_22kHzMono, SITooQuiet,
    ISpStreamFormatConverter, DISPID_SGRName, SP_VISEME_14,
    DISPID_SGRInitialState, SAFT44kHz16BitStereo, VARIANT_BOOL,
    DISPID_SPIAudioSizeTime, DISPID_SRCCreateGrammar, SVEVoiceChange,
    SAFTADPCM_8kHzStereo, SpSharedRecoContext, SVSFDefault, SASStop,
    SREPhraseStart, DISPID_SGRSTPropertyId, SP_VISEME_9, COMMETHOD,
    SAFTCCITT_ALaw_11kHzStereo, ISpeechVoiceStatus, SPVPRI_ALERT,
    SPTEXTSELECTIONINFO, SpInprocRecognizer,
    SPWP_UNKNOWN_WORD_PRONOUNCEABLE, SVPOver, SRAONone,
    DISPID_SOTMatchesAttributes, SpVoice, SFTInput,
    SAFT32kHz16BitStereo, DISPID_SLPType, SRTAutopause,
    SAFTNoAssignedFormat, SpeechTokenIdUserLexicon, SSTTWildcard,
    SPPROPERTYINFO, DISPID_SOTCreateInstance, SRADefaultToActive,
    ISpAudio, DISPID_SWFEBlockAlign, _check_version, SPEI_MIN_SR,
    SpeechGrammarTagUnlimitedDictation, ISpeechPhraseInfoBuilder,
    SpeechAudioVolume, SPEI_RECO_STATE_CHANGE, SPGS_EXCLUSIVE,
    DISPID_SVSkip, DISPID_SRCEEnginePrivate, DISPID_SPAPhraseInfo,
    SPINTERFERENCE_LATENCY_TRUNCATE_END,
    DISPID_SRCCreateResultFromMemory, SpShortcut,
    DISPID_SLPPartOfSpeech, SRTExtendableParse, ISpeechPhraseProperty,
    ISpXMLRecoResult, STCInprocServer, DISPID_SPILanguageId,
    DISPID_SLRemovePronunciationByPhoneIds, HRESULT,
    DISPID_SWFEBitsPerSample, STCAll, DISPID_SRDisplayUI,
    SWPKnownWordPronounceable, DISPID_SGRSTPropertyName,
    SpeechPropertyResponseSpeed, ISpeechLexiconPronunciation,
    SpObjectTokenCategory, DISPID_SRGetPropertyNumber,
    DISPID_SRCEventInterests, DISPID_SRCESoundStart,
    SAFTGSM610_44kHzMono, SPAUDIOSTATUS, DISPID_SASetState,
    SpeechCategoryVoices, SPFM_OPEN_READWRITE, Speech_StreamPos_Asap,
    SPLO_STATIC, SREAdaptation, eLEXTYPE_RESERVED7,
    eLEXTYPE_PRIVATE11, DISPID_SPPName, SpResourceManager, SVP_17,
    SPCS_DISABLED, SVSFIsFilename, SPAS_CLOSED, SPSMF_UPS, SPAR_High,
    DISPID_SRCEEndStream, SPSUnknown, SPBO_AHEAD, SP_VISEME_7,
    DISPID_SLPsCount, SAFTGSM610_11kHzMono, SPCT_COMMAND,
    SPEI_INTERFERENCE, SVP_18, DISPID_SDKEnumKeys, DISPID_SLWLangId,
    SVSFParseAutodetect, SP_VISEME_20, DISPID_SPEPronunciation,
    DISPID_SRCEAudioLevel, DISPID_SGRSTPropertyValue, SRSInactive,
    SAFTCCITT_uLaw_44kHzMono, SAFT12kHz16BitMono,
    SAFTGSM610_22kHzMono, DISPID_SVPause, SREStreamEnd,
    SpUnCompressedLexicon, Speech_Max_Pron_Length,
    SAFTADPCM_44kHzMono, SPWORD, SpLexicon, DISPID_SVEEnginePrivate,
    ISpeechRecoResult2, DISPID_SVEStreamStart, SLTApp,
    DISPID_SAFGetWaveFormatEx, DISPID_SLWsCount, SPRST_ACTIVE_ALWAYS,
    DISPID_SLWsItem, DISPID_SPRules_NewEnum, ISpRecognizer2,
    SPEI_ACTIVE_CATEGORY_CHANGED, DISPID_SPIGrammarId,
    DISPID_SRGCmdLoadFromObject, SpeechGrammarTagWildcard,
    DISPID_SGRsCommit, DISPID_SPIStartTime, ISpeechLexiconWords,
    SAFTADPCM_44kHzStereo, SPEI_RESERVED3, DISPID_SMSAMMHandle,
    SAFT12kHz16BitStereo, DISPID_SVSPhonemeId, DISPID_SVAlertBoundary,
    SpPhoneticAlphabetConverter, DISPID_SVGetAudioInputs,
    SAFTCCITT_ALaw_22kHzStereo, SPAS_PAUSE, eLEXTYPE_RESERVED4,
    SAFT22kHz8BitStereo, SAFT22kHz16BitStereo, SVP_0, SPXRO_SML,
    SPFM_CREATE, DISPID_SRCSetAdaptationData, DISPID_SPRText,
    SGSExclusive, SpeechPropertyComplexResponseSpeed, ISpRecoCategory,
    SPPS_LMA, SPINTERFERENCE_NOISE, STCRemoteServer, SRAInterpreter,
    DISPID_SGRSTsItem, DISPID_SLAddPronunciationByPhoneIds,
    DISPID_SPIGetDisplayAttributes, DISPID_SPRuleName, DISPID_SLWType,
    ISpObjectWithToken, DISPID_SGRSAddSpecialTransition, SRARoot,
    ISpPhoneticAlphabetSelection, DISPID_SPIEnginePrivateData,
    ISpRecoContext2, DISPID_SRCEPhraseStart, DISPID_SPRsItem,
    eLEXTYPE_LETTERTOSOUND, SECFNoSpecialChars, ISpeechResourceLoader,
    SSFMCreate, DISPID_SPPsItem, SVEAllEvents, SPVOICESTATUS, VARIANT,
    SpeechCategoryRecoProfiles, DISPID_SRCERequestUI, DISPID_SRRTimes,
    SPAO_NONE, SPEVENTSOURCEINFO, SINoSignal, SAFT24kHz8BitMono,
    SDTLexicalForm, SPWORDPRONUNCIATIONLIST, DISPID_SPRuleChildren,
    SPSHT_Unknown, SVP_4, SAFT44kHz16BitMono, SPEI_VISEME,
    ISpeechTextSelectionInformation, SREAudioLevel, SPGS_ENABLED,
    DISPID_SRCVoice, SGRSTTEpsilon, SPEI_RESERVED6,
    DISPID_SLPs_NewEnum, DISPID_SRRRecoContext, SAFT16kHz8BitStereo,
    __MIDL_IWinTypes_0009, SDTPronunciation, DISPID_SRRecognizer,
    SPINTERFERENCE_NONE, DISPID_SOTGetStorageFileName, SP_VISEME_16,
    eLEXTYPE_PRIVATE12, SpMemoryStream, DISPID_SVEWord,
    SpeechUserTraining, DISPID_SPRuleConfidence,
    DISPID_SRGDictationSetState, SVP_13, SRESoundStart,
    SWPUnknownWordUnpronounceable, SAFTCCITT_ALaw_44kHzMono,
    SP_VISEME_1, SPEI_HYPOTHESIS, ISpeechXMLRecoResult, SASRun,
    DISPID_SRGReset, ISpeechPhraseReplacement, SP_VISEME_6,
    eLEXTYPE_PRIVATE17, SVEWordBoundary, _ISpeechVoiceEvents, SVP_8,
    ISpeechRecoResultTimes, wireHWND, DISPID_SPEs_NewEnum,
    SGRSTTTextBuffer, SpAudioFormat, eLEXTYPE_RESERVED10,
    SPEI_RECOGNITION, SFTSREngine, SRCS_Enabled, BSTR,
    DISPID_SRRSaveToMemory, SpeechEngineProperties, DISPMETHOD,
    eLEXTYPE_PRIVATE10, SVEAudioLevel, DISPID_SRCEBookmark,
    STSF_AppData, DISPID_SPACommit, DISPID_SPIGetText, SPEI_PHONEME,
    DISPID_SRGetPropertyString, DISPID_SRCBookmark,
    ISpeechPhraseElement, SpMMAudioOut, eLEXTYPE_RESERVED6,
    DISPID_SRRTOffsetFromStart, DISPID_SDKSetBinaryValue,
    DISPID_SVIsUISupported, DISPID_SPCPhoneToId, DISPID_SGRClear,
    SAFTGSM610_8kHzMono, DISPID_SGRsCount,
    DISPID_SOTRemoveStorageFileName, SAFT12kHz8BitStereo,
    DISPID_SREmulateRecognition, SITooSlow, DISPID_SRCRetainedAudio,
    SP_VISEME_19, SECFEmulateResult, SPFM_NUM_MODES, SP_VISEME_8,
    DISPID_SVSLastResult, SPPS_Noncontent, SPSHT_NotOverriden,
    eLEXTYPE_PRIVATE19, UINT_PTR, DISPID_SLPsItem,
    SpStreamFormatConverter, ISpSerializeState, DISPID_SPEsItem,
    SPPHRASEELEMENT, SVEPhoneme, SPSMF_SAPI_PROPERTIES,
    SAFTTrueSpeech_8kHz1BitMono, SECFIgnoreKanaType,
    SPCT_SUB_DICTATION, SRTSMLTimeout, DISPID_SLGetGenerationChange,
    SVSFNLPMask, SVPAlert, SAFT24kHz16BitMono, SPSLMA, SVEPrivate,
    DISPID_SOTId, DISPID_SDKSetStringValue, SVP_11,
    IInternetSecurityManager, SPPS_Noun, SPEI_MAX_SR, GUID,
    SPEI_FALSE_RECOGNITION, SPAR_Low, SPFM_CREATE_ALWAYS,
    eLEXTYPE_PRIVATE2, SpeechVoiceSkipTypeSentence,
    SPFM_OPEN_READONLY, DISPID_SRGId, DISPID_SRAudioInput,
    DISPID_SVSInputSentencePosition,
    SPSMF_SRGS_SEMANTICINTERPRETATION_W3C, DISPID_SPISaveToMemory,
    DISPID_SVEVoiceChange, SVP_19, SPCT_DICTATION,
    DISPID_SGRs_NewEnum, SAFT12kHz8BitMono, DISPID_SRRTTickCount,
    DISPID_SRGCmdLoadFromMemory, DISPID_SPEEngineConfidence,
    ISpeechRecoGrammar, DISPID_SDKGetStringValue, SP_VISEME_0,
    DISPID_SRSAudioStatus, SpeechRegistryLocalMachineRoot,
    SSFMCreateForWrite, SPSMF_SRGS_SAPIPROPERTIES, DISPID_SGRsDynamic,
    DISPID_SRRGetXMLErrorInfo, SAFT16kHz16BitStereo, SPRST_ACTIVE,
    SITooLoud, SPSHORTCUTPAIR, SREPropertyStringChange,
    SECNormalConfidence, SECFIgnoreWidth, SBONone, SP_VISEME_21,
    SPRULE, DISPID_SVSpeakCompleteEvent, ISpeechLexiconPronunciations,
    DISPID_SPIElements,
    DISPID_SRAllowAudioInputFormatChangesOnNextSet,
    DISPID_SRSCurrentStreamNumber, DISPID_SVESentenceBoundary,
    SpeechCategoryAudioOut, DISPID_SGRSTsCount, WSTRING,
    SpNotifyTranslator, DISPID_SPAs_NewEnum, SpNullPhoneConverter,
    SRERecoOtherContext, ISpRecoContext, ISpNotifyTranslator,
    DISPID_SVWaitUntilDone, SPEI_PROPERTY_STRING_CHANGE, SPWT_DISPLAY,
    DISPID_SGRsItem, SSFMOpenForRead, SPEI_UNDEFINED, SGRSTTWildcard,
    ISpeechPhraseInfo, SVESentenceBoundary, DISPID_SRGDictationUnload,
    SDKLLocalMachine, SAFTCCITT_ALaw_8kHzStereo, SPPS_RESERVED2,
    SGSEnabled, SREPrivate, SpeechMicTraining, DISPID_SGRSTWeight,
    DISPID_SPPsCount, SLOStatic, SP_VISEME_5, DISPID_SRIsUISupported,
    SVP_7, SPWF_INPUT, SVP_9, SpCustomStream, SAFTCCITT_ALaw_8kHzMono,
    DISPID_SPPFirstElement, SRSEIsSpeaking, SDTReplacement,
    DISPID_SBSSeek, DISPID_SAEventHandle, SVP_20, DISPID_SLGetWords,
    __MIDL___MIDL_itf_sapi_0000_0020_0001,
    DISPID_SRCEFalseRecognition, SPINTERFERENCE_LATENCY_WARNING,
    SAFT32kHz16BitMono, SASClosed, DISPID_SPERetainedStreamOffset,
    DISPID_SGRSRule, SpeechAudioFormatGUIDWave, DISPID_SPPValue,
    SpStream, SAFTADPCM_22kHzMono, SPGS_DISABLED, ISpGrammarBuilder,
    ISpeechAudio, DISPID_SVEViseme, DISPID_SRGDictationLoad,
    SPEI_END_INPUT_STREAM, DISPID_SGRsFindRule, DISPID_SLPPhoneIds,
    SDTRule, ISpeechDataKey, DISPID_SPCLangId, ISpRecoResult,
    SREAllEvents, DISPID_SPRulesCount, SPEI_PROPERTY_NUM_CHANGE,
    eLEXTYPE_PRIVATE1, SAFTExtendedAudioFormat,
    SpeechTokenKeyAttributes, DISPID_SPERequiredConfidence,
    DISPID_SRIsShared, DISPID_SPIProperties,
    SPRS_ACTIVE_USER_DELIMITED, SVSFNLPSpeakPunc, SVSFPersistXML,
    DISPID_SVAllowAudioOuputFormatChangesOnNextSet, SPEI_MAX_TTS,
    SPWORDPRONUNCIATION, DISPID_SABufferInfo, SPSInterjection,
    DISPID_SVEBookmark, SPPS_NotOverriden, DISPID_SAFType,
    SPEI_SOUND_START, DISPID_SRCRequestedUIType,
    DISPID_SRGetRecognizers, SPEI_PHRASE_START,
    tagSPTEXTSELECTIONINFO, ISpeechRecoResult, SRSInactiveWithPurge,
    DISPID_SASCurrentSeekPosition, DISPID_SGRSTType,
    DISPID_SRAudioInputStream, helpstring, SVSFIsXML, SPBINARYGRAMMAR,
    SPWP_KNOWN_WORD_PRONOUNCEABLE, DISPID_SPEActualConfidence,
    eLEXTYPE_PRIVATE13, IServiceProvider, SVP_14, DISPID_SVSpeak,
    SVSFlagsAsync, DISPID_SPPs_NewEnum, DISPID_SVEAudioLevel,
    SAFTADPCM_22kHzStereo, SPEI_RESERVED1, DISPID_SDKSetLongValue,
    DISPID_SWFEAvgBytesPerSec, DISPID_SOTIsUISupported, SVF_None,
    DISPID_SPPConfidence, SAFT11kHz8BitStereo, DISPID_SOTCGetDataKey,
    SpCompressedLexicon, DISPID_SGRSTNextState, DISPID_SVStatus,
    SPEI_REQUEST_UI, DISPID_SLPLangId, SRERequestUI, DISPID_SRCPause,
    DISPID_SAFGuid, DISPID_SVSLastStreamNumberQueued,
    DISPID_SVAudioOutput, DISPID_SRRSetTextFeedback, SECLowConfidence,
    SpeechPropertyAdaptationOn, SINone, SAFT48kHz8BitStereo,
    SAFT8kHz8BitMono, DISPID_SCSBaseStream, CoClass, SPRS_ACTIVE,
    DISPID_SMSADeviceId, eLEXTYPE_RESERVED8, SSTTDictation,
    SPAO_RETAIN_AUDIO, SDKLCurrentConfig, SAFTADPCM_11kHzStereo,
    DISPIDSPTSI_ActiveLength, SREFalseRecognition, SPVPRI_OVER, SVP_5,
    SPPS_SuppressWord, SRATopLevel, ISpeechObjectTokenCategory,
    ISpObjectToken, SPPHRASEREPLACEMENT, ISpeechGrammarRuleState,
    DISPID_SPELexicalForm, DISPID_SRCESoundEnd, SLTUser,
    SPWP_UNKNOWN_WORD_UNPRONOUNCEABLE, SDA_One_Trailing_Space,
    DISPID_SVSLastBookmark, SPEVENT, SAFT16kHz16BitMono,
    DISPID_SOTDataKey, DISPID_SRGCommit, SPCS_ENABLED,
    DISPID_SGRSTText, SAFT16kHz8BitMono, DISPID_SPRuleId,
    DISPID_SRCCmdMaxAlternates, SVSFParseSapi, SP_VISEME_17,
    SVEViseme, DISPID_SABIMinNotification, SpeechCategoryAppLexicons,
    SVSFPurgeBeforeSpeak, DISPID_SOTCDefault,
    DISPID_SRCEPropertyStringChange, ISpDataKey,
    DISPID_SRCERecognizerStateChange,
    __MIDL___MIDL_itf_sapi_0000_0020_0002, DISPID_SPRulesItem,
    IEnumSpObjectTokens, DISPID_SRGSetTextSelection,
    ISpResourceManager, DISPID_SOTGetAttribute,
    DISPID_SABufferNotifySize, DISPID_SGRSAddRuleTransition,
    SP_VISEME_12, SAFT8kHz8BitStereo, SDTAudio,
    SAFTCCITT_uLaw_22kHzMono, SPWF_SRENGINE, DISPID_SWFEFormatTag,
    SPEI_WORD_BOUNDARY, ISpeechRecognizerStatus, DISPID_SMSSetData,
    IStream, DISPID_SRCRetainedAudioFormat, SVEStartInputStream,
    DISPID_SOTSetId, DISPID_SPIEngineId, ISpEventSource, WAVEFORMATEX,
    ISpeechRecognizer, SAFTNonStandardFormat, _LARGE_INTEGER,
    SRSActive, SPSNotOverriden, SSSPTRelativeToEnd,
    SpeechPropertyResourceUsage, SAFT44kHz8BitMono,
    DISPID_SRGCmdLoadFromProprietaryGrammar, SAFTCCITT_uLaw_8kHzMono,
    SDKLCurrentUser, DISPID_SRCVoicePurgeEvent,
    DISPID_SRSSupportedLanguages, DISPID_SRCRecognizer, SECFDefault,
    DISPID_SOTCategory, SPSHT_OTHER, DISPID_SOTsItem,
    eLEXTYPE_USER_SHORTCUT, DISPID_SMSALineId, DISPID_SDKEnumValues,
    DISPID_SOTGetDescription, DISPID_SRGSetWordSequenceData,
    SPEI_START_SR_STREAM, DISPID_SABIBufferSize, DISPID_SBSWrite,
    DISPID_SFSOpen, SRESoundEnd, SVSFParseMask, dispid,
    SSSPTRelativeToStart, SLODynamic, SPEI_START_INPUT_STREAM,
    ULONG_PTR, DISPID_SRSNumberOfActiveRules, ISpeechPhraseElements,
    DISPID_SADefaultFormat, ISpeechPhraseAlternate, IUnknown,
    SAFT44kHz8BitStereo, DISPID_SRCEAdaptation, SECHighConfidence,
    eLEXTYPE_PRIVATE7, _ULARGE_INTEGER, DISPID_SDKOpenKey,
    SAFT32kHz8BitMono, SpeechCategoryAudioIn, DISPID_SRRAudio,
    DISPID_SVSRunningState, DISPID_SPAsItem, SPPS_RESERVED4,
    SP_VISEME_3, DISPID_SOTs_NewEnum, SPRS_ACTIVE_WITH_AUTO_PAUSE,
    SGDisplay, _ISpeechRecoContextEvents, STSF_LocalAppData,
    DISPID_SRCreateRecoContext, DISPID_SVSInputWordLength,
    DISPID_SDKGetBinaryValue, SVSFVoiceMask, eLEXTYPE_APP,
    SPPHRASERULE, DISPID_SRCResume, ISpStreamFormat,
    DISPID_SRCEInterference, ISpeechAudioBufferInfo, SVF_Emphasis,
    ISpeechPhraseAlternates, eLEXTYPE_PRIVATE14, DISPID_SLWs_NewEnum,
    SAFTCCITT_uLaw_11kHzStereo, DISPID_SRGCmdSetRuleState,
    DISPID_SPIAudioSizeBytes, ISpRecognizer, SpeechAllElements,
    DISPID_SVAudioOutputStream, SPCT_SUB_COMMAND, eWORDTYPE_DELETED,
    DISPID_SPCIdToPhone, SPEI_RECO_OTHER_CONTEXT, SPPS_Function,
    SREInterference, SPBO_PAUSE, Library, SAFTADPCM_11kHzMono,
    SDA_Consume_Leading_Spaces, ISpeechRecoContext,
    eLEXTYPE_PRIVATE16, SAFT22kHz8BitMono, SAFT32kHz8BitStereo,
    DISPID_SVDisplayUI, DISPID_SLPSymbolic, DISPID_SVEPhoneme,
    DISPID_SPPBRestorePhraseFromMemory, SRSEDone, SPAS_STOP,
    SPCT_SLEEP, DISPID_SRSetPropertyNumber, SpMMAudioIn,
    DISPID_SWFESamplesPerSec, SPSHT_EMAIL, DISPID_SPPId,
    ISpRecoGrammar2, SAFTCCITT_uLaw_8kHzStereo, SRSActiveAlways,
    SP_VISEME_11, SPBO_NONE, SPEI_VOICE_CHANGE, SWTAdded,
    SPEI_SR_AUDIO_LEVEL, SpeechPropertyHighConfidenceThreshold,
    DISPID_SDKDeleteKey, ISpeechLexiconWord, SPDKL_CurrentUser,
    DISPID_SOTsCount, SPINTERFERENCE_NOSIGNAL, SPINTERFERENCE_TOOLOUD,
    SPDKL_LocalMachine, SAFT48kHz8BitMono,
    DISPID_SPIRetainedSizeBytes, SVEBookmark, ISpMMSysAudio,
    ISpeechRecoResultDispatch, DISPIDSPTSI_SelectionLength,
    SPRECOGNIZERSTATUS, DISPID_SRCERecognitionForOtherContext,
    SpeechTokenKeyUI, SPRST_INACTIVE_WITH_PURGE, SVP_16,
    SSFMOpenReadWrite, DISPID_SOTCId, SAFT24kHz16BitStereo,
    DISPID_SPPEngineConfidence, DISPID_SRProfile,
    SAFTCCITT_ALaw_11kHzMono, DISPID_SRCEPropertyNumberChange,
    SPRST_INACTIVE, DISPID_SRRTStreamTime, SpeechAudioProperties,
    _FILETIME, SP_VISEME_10, ISpPhoneticAlphabetConverter, SRADynamic,
    DISPID_SGRAddState, Speech_StreamPos_RealTime,
    SAFT24kHz8BitStereo, DISPID_SPERetainedSizeBytes, SRAImport,
    SpeechAddRemoveWord, DISPID_SVSyncronousSpeakTimeout,
    SPEI_END_SR_STREAM, SVP_15, DISPID_SPEAudioTimeOffset,
    DISPID_SGRsAdd, ISpEventSink, SpMMAudioEnum,
    DISPID_SASCurrentDevicePosition, DISPIDSPTSI_ActiveOffset, _lcid,
    DISPID_SPAsCount, SVSFIsNotXML, ISpeechGrammarRules,
    ISpeechPhraseProperties, SpeechTokenKeyFiles, SVP_3,
    SpeechVoiceCategoryTTSRate, SRCS_Disabled,
    DISPID_SVSInputSentenceLength, SpPhoneConverter, typelib_path,
    DISPID_SPIAudioStreamPosition, DISPID_SBSFormat, SPEI_MIN_TTS,
    SPSERIALIZEDRESULT, DISPID_SLWWord, SpeechCategoryRecognizers,
    ISpeechVoice, DISPID_SRRGetXMLResult, DISPID_SPRs_NewEnum,
    DISPID_SLGetPronunciations, DISPID_SVEStreamEnd,
    DISPID_SRRDiscardResultInfo, eLEXTYPE_PRIVATE18,
    DISPID_SRCAudioInInterferenceStatus, Speech_Max_Word_Length,
    SVF_Stressed, SWPUnknownWordPronounceable, SPPS_Interjection,
    ISpStream, DISPID_SVSCurrentStreamNumber,
    SAFTCCITT_ALaw_44kHzStereo, DISPID_SVResume,
    DISPID_SOTCEnumerateTokens, DISPID_SRGCmdSetRuleIdState,
    SREBookmark, SPSNoun, DISPID_SVGetVoices,
    DISPID_SLWPronunciations, SPEI_ADAPTATION, ISpRecognizer3,
    SpeechAudioFormatGUIDText, SVP_2, ISpeechPhraseRule, SRTStandard,
    SINoise, SAFT22kHz16BitMono, SVP_10, SPSModifier,
    DISPID_SPRuleEngineConfidence, DISPID_SPIRule, SP_VISEME_18,
    SpObjectToken, ISequentialStream, SRAExport, _RemotableHandle,
    DISPID_SRRAlternates, eWORDTYPE_ADDED, DISPID_SPRuleFirstElement,
    DISPID_SPANumberOfElementsInResult, STCLocalServer, SRTReSent,
    SRAORetainAudio, SPSSuppressWord, DISPID_SRSetPropertyString,
    DISPID_SGRId, DISPID_SPAStartElementInResult, SPSERIALIZEDPHRASE,
    SPINTERFERENCE_LATENCY_TRUNCATE_BEGIN, IInternetSecurityMgrSite,
    eLEXTYPE_PRIVATE9, DISPID_SAVolume, ISpeechLexicon,
    DISPID_SRGetFormat, ISpNotifySink, SPAR_Unknown,
    SAFT8kHz16BitMono, SAFT48kHz16BitStereo, DISPID_SGRAttributes,
    SGPronounciation, eLEXTYPE_PRIVATE6, DISPID_SVGetAudioOutputs,
    SpeechCategoryPhoneConverters, DISPID_SRCState,
    DISPID_SRGIsPronounceable, STCInprocHandler,
    DISPID_SRAllowVoiceFormatMatchingOnNextSet, SGRSTTRule,
    SRTEmulated, ISpeechAudioStatus, DISPID_SRCEStartStream,
    SPEI_SENTENCE_BOUNDARY, SpPhraseInfoBuilder, DISPID_SPPChildren,
    DISPID_SGRSAddWordTransition, SGSDisabled, DISPID_SPRuleParent,
    SpWaveFormatEx, DISPID_SBSRead, DISPID_SRRSpeakAudio,
    eLEXTYPE_PRIVATE8, ISpeechWaveFormatEx, ISpeechPhoneConverter,
    ISpPhrase, SPINTERFERENCE_TOOFAST, DISPID_SLRemovePronunciation,
    SPPHRASE, ISpeechGrammarRuleStateTransition,
    DISPID_SVSLastBookmarkId, DISPID_SRGRecoContext,
    eLEXTYPE_PRIVATE5, SDTDisplayText, SpeechDictationTopicSpelling,
    eLEXTYPE_PRIVATE20, DISPID_SRState,
    DISPID_SRSCurrentStreamPosition, SpSharedRecognizer,
    DISPID_SMSGetData, SREStreamStart, SpTextSelectionInformation,
    SPRECORESULTTIMES, SPEI_SOUND_END, SpFileStream,
    DISPID_SRRTLength, SAFTDefault, DISPID_SPRuleNumberOfElements,
    DISPID_SRRAudioFormat, DISPID_SPPNumberOfElements, SPSVerb,
    DISPID_SVRate, STSF_CommonAppData
)


class SpeechRetainedAudioOptions(IntFlag):
    SRAONone = 0
    SRAORetainAudio = 1


class SpeechGrammarState(IntFlag):
    SGSEnabled = 1
    SGSDisabled = 0
    SGSExclusive = 3


class SPSHORTCUTTYPE(IntFlag):
    SPSHT_NotOverriden = -1
    SPSHT_Unknown = 0
    SPSHT_EMAIL = 4096
    SPSHT_OTHER = 8192
    SPPS_RESERVED1 = 12288
    SPPS_RESERVED2 = 16384
    SPPS_RESERVED3 = 20480
    SPPS_RESERVED4 = 61440


class SpeechDisplayAttributes(IntFlag):
    SDA_No_Trailing_Space = 0
    SDA_One_Trailing_Space = 2
    SDA_Two_Trailing_Spaces = 4
    SDA_Consume_Leading_Spaces = 8


class SPLEXICONTYPE(IntFlag):
    eLEXTYPE_USER = 1
    eLEXTYPE_APP = 2
    eLEXTYPE_VENDORLEXICON = 4
    eLEXTYPE_LETTERTOSOUND = 8
    eLEXTYPE_MORPHOLOGY = 16
    eLEXTYPE_RESERVED4 = 32
    eLEXTYPE_USER_SHORTCUT = 64
    eLEXTYPE_RESERVED6 = 128
    eLEXTYPE_RESERVED7 = 256
    eLEXTYPE_RESERVED8 = 512
    eLEXTYPE_RESERVED9 = 1024
    eLEXTYPE_RESERVED10 = 2048
    eLEXTYPE_PRIVATE1 = 4096
    eLEXTYPE_PRIVATE2 = 8192
    eLEXTYPE_PRIVATE3 = 16384
    eLEXTYPE_PRIVATE4 = 32768
    eLEXTYPE_PRIVATE5 = 65536
    eLEXTYPE_PRIVATE6 = 131072
    eLEXTYPE_PRIVATE7 = 262144
    eLEXTYPE_PRIVATE8 = 524288
    eLEXTYPE_PRIVATE9 = 1048576
    eLEXTYPE_PRIVATE10 = 2097152
    eLEXTYPE_PRIVATE11 = 4194304
    eLEXTYPE_PRIVATE12 = 8388608
    eLEXTYPE_PRIVATE13 = 16777216
    eLEXTYPE_PRIVATE14 = 33554432
    eLEXTYPE_PRIVATE15 = 67108864
    eLEXTYPE_PRIVATE16 = 134217728
    eLEXTYPE_PRIVATE17 = 268435456
    eLEXTYPE_PRIVATE18 = 536870912
    eLEXTYPE_PRIVATE19 = 1073741824
    eLEXTYPE_PRIVATE20 = -2147483648


class SPPARTOFSPEECH(IntFlag):
    SPPS_NotOverriden = -1
    SPPS_Unknown = 0
    SPPS_Noun = 4096
    SPPS_Verb = 8192
    SPPS_Modifier = 12288
    SPPS_Function = 16384
    SPPS_Interjection = 20480
    SPPS_Noncontent = 24576
    SPPS_LMA = 28672
    SPPS_SuppressWord = 61440


class SPSEMANTICFORMAT(IntFlag):
    SPSMF_SAPI_PROPERTIES = 0
    SPSMF_SRGS_SEMANTICINTERPRETATION_MS = 1
    SPSMF_SRGS_SAPIPROPERTIES = 2
    SPSMF_UPS = 4
    SPSMF_SRGS_SEMANTICINTERPRETATION_W3C = 8


class SPWORDTYPE(IntFlag):
    eWORDTYPE_ADDED = 1
    eWORDTYPE_DELETED = 2


class SpeechRuleAttributes(IntFlag):
    SRATopLevel = 1
    SRADefaultToActive = 2
    SRAExport = 4
    SRAImport = 8
    SRAInterpreter = 16
    SRADynamic = 32
    SRARoot = 64


class SPGRAMMARWORDTYPE(IntFlag):
    SPWT_DISPLAY = 0
    SPWT_LEXICAL = 1
    SPWT_PRONUNCIATION = 2
    SPWT_LEXICAL_NO_SPECIAL_CHARS = 3


class SpeechTokenContext(IntFlag):
    STCInprocServer = 1
    STCInprocHandler = 2
    STCLocalServer = 4
    STCRemoteServer = 16
    STCAll = 23


class SpeechTokenShellFolder(IntFlag):
    STSF_AppData = 26
    STSF_LocalAppData = 28
    STSF_CommonAppData = 35
    STSF_FlagCreate = 32768


class SPLOADOPTIONS(IntFlag):
    SPLO_STATIC = 0
    SPLO_DYNAMIC = 1


class SpeechLoadOption(IntFlag):
    SLOStatic = 0
    SLODynamic = 1


class SpeechRuleState(IntFlag):
    SGDSInactive = 0
    SGDSActive = 1
    SGDSActiveWithAutoPause = 3
    SGDSActiveUserDelimited = 4


class SpeechWordPronounceable(IntFlag):
    SWPUnknownWordUnpronounceable = 0
    SWPUnknownWordPronounceable = 1
    SWPKnownWordPronounceable = 2


class SPRULESTATE(IntFlag):
    SPRS_INACTIVE = 0
    SPRS_ACTIVE = 1
    SPRS_ACTIVE_WITH_AUTO_PAUSE = 3
    SPRS_ACTIVE_USER_DELIMITED = 4


class SPWORDPRONOUNCEABLE(IntFlag):
    SPWP_UNKNOWN_WORD_UNPRONOUNCEABLE = 0
    SPWP_UNKNOWN_WORD_PRONOUNCEABLE = 1
    SPWP_KNOWN_WORD_PRONOUNCEABLE = 2


class SPGRAMMARSTATE(IntFlag):
    SPGS_DISABLED = 0
    SPGS_ENABLED = 1
    SPGS_EXCLUSIVE = 3


class SpeechDataKeyLocation(IntFlag):
    SDKLDefaultLocation = 0
    SDKLCurrentUser = 1
    SDKLLocalMachine = 2
    SDKLCurrentConfig = 5


class SPINTERFERENCE(IntFlag):
    SPINTERFERENCE_NONE = 0
    SPINTERFERENCE_NOISE = 1
    SPINTERFERENCE_NOSIGNAL = 2
    SPINTERFERENCE_TOOLOUD = 3
    SPINTERFERENCE_TOOQUIET = 4
    SPINTERFERENCE_TOOFAST = 5
    SPINTERFERENCE_TOOSLOW = 6
    SPINTERFERENCE_LATENCY_WARNING = 7
    SPINTERFERENCE_LATENCY_TRUNCATE_BEGIN = 8
    SPINTERFERENCE_LATENCY_TRUNCATE_END = 9


class SpeechEngineConfidence(IntFlag):
    SECLowConfidence = -1
    SECNormalConfidence = 0
    SECHighConfidence = 1


class SPAUDIOOPTIONS(IntFlag):
    SPAO_NONE = 0
    SPAO_RETAIN_AUDIO = 1


class SPXMLRESULTOPTIONS(IntFlag):
    SPXRO_SML = 0
    SPXRO_Alternates_SML = 1


class SpeechGrammarWordType(IntFlag):
    SGDisplay = 0
    SGLexical = 1
    SGPronounciation = 2
    SGLexicalNoSpecialChars = 3


class SpeechSpecialTransitionType(IntFlag):
    SSTTWildcard = 1
    SSTTDictation = 2
    SSTTTextBuffer = 3


class SPBOOKMARKOPTIONS(IntFlag):
    SPBO_NONE = 0
    SPBO_PAUSE = 1
    SPBO_AHEAD = 2
    SPBO_TIME_UNITS = 4


class SPCONTEXTSTATE(IntFlag):
    SPCS_DISABLED = 0
    SPCS_ENABLED = 1


class SPADAPTATIONRELEVANCE(IntFlag):
    SPAR_Unknown = 0
    SPAR_Low = 1
    SPAR_Medium = 2
    SPAR_High = 3


class SPCATEGORYTYPE(IntFlag):
    SPCT_COMMAND = 0
    SPCT_DICTATION = 1
    SPCT_SLEEP = 2
    SPCT_SUB_COMMAND = 3
    SPCT_SUB_DICTATION = 4


class SpeechStreamSeekPositionType(IntFlag):
    SSSPTRelativeToStart = 0
    SSSPTRelativeToCurrentPosition = 1
    SSSPTRelativeToEnd = 2


class SpeechBookmarkOptions(IntFlag):
    SBONone = 0
    SBOPause = 1


class SpeechRecognitionType(IntFlag):
    SRTStandard = 0
    SRTAutopause = 1
    SRTEmulated = 2
    SRTSMLTimeout = 4
    SRTExtendableParse = 8
    SRTReSent = 16


class SpeechInterference(IntFlag):
    SINone = 0
    SINoise = 1
    SINoSignal = 2
    SITooLoud = 3
    SITooQuiet = 4
    SITooFast = 5
    SITooSlow = 6


class SpeechRecognizerState(IntFlag):
    SRSInactive = 0
    SRSActive = 1
    SRSActiveAlways = 2
    SRSInactiveWithPurge = 3


class SpeechStreamFileMode(IntFlag):
    SSFMOpenForRead = 0
    SSFMOpenReadWrite = 1
    SSFMCreate = 2
    SSFMCreateForWrite = 3


class SpeechVoiceEvents(IntFlag):
    SVEStartInputStream = 2
    SVEEndInputStream = 4
    SVEVoiceChange = 8
    SVEBookmark = 16
    SVEWordBoundary = 32
    SVEPhoneme = 64
    SVESentenceBoundary = 128
    SVEViseme = 256
    SVEAudioLevel = 512
    SVEPrivate = 32768
    SVEAllEvents = 33790


class SpeechVoicePriority(IntFlag):
    SVPNormal = 0
    SVPAlert = 1
    SVPOver = 2


class SpeechVisemeFeature(IntFlag):
    SVF_None = 0
    SVF_Stressed = 1
    SVF_Emphasis = 2


class SpeechVisemeType(IntFlag):
    SVP_0 = 0
    SVP_1 = 1
    SVP_2 = 2
    SVP_3 = 3
    SVP_4 = 4
    SVP_5 = 5
    SVP_6 = 6
    SVP_7 = 7
    SVP_8 = 8
    SVP_9 = 9
    SVP_10 = 10
    SVP_11 = 11
    SVP_12 = 12
    SVP_13 = 13
    SVP_14 = 14
    SVP_15 = 15
    SVP_16 = 16
    SVP_17 = 17
    SVP_18 = 18
    SVP_19 = 19
    SVP_20 = 20
    SVP_21 = 21


class SpeechAudioState(IntFlag):
    SASClosed = 0
    SASStop = 1
    SASPause = 2
    SASRun = 3


class SpeechLexiconType(IntFlag):
    SLTUser = 1
    SLTApp = 2


class SpeechPartOfSpeech(IntFlag):
    SPSNotOverriden = -1
    SPSUnknown = 0
    SPSNoun = 4096
    SPSVerb = 8192
    SPSModifier = 12288
    SPSFunction = 16384
    SPSInterjection = 20480
    SPSLMA = 28672
    SPSSuppressWord = 61440


class SpeechGrammarRuleStateTransitionType(IntFlag):
    SGRSTTEpsilon = 0
    SGRSTTWord = 1
    SGRSTTRule = 2
    SGRSTTDictation = 3
    SGRSTTWildcard = 4
    SGRSTTTextBuffer = 5


class SpeechRunState(IntFlag):
    SRSEDone = 1
    SRSEIsSpeaking = 2


class SpeechFormatType(IntFlag):
    SFTInput = 0
    SFTSREngine = 1


class SpeechAudioFormatType(IntFlag):
    SAFTDefault = -1
    SAFTNoAssignedFormat = 0
    SAFTText = 1
    SAFTNonStandardFormat = 2
    SAFTExtendedAudioFormat = 3
    SAFT8kHz8BitMono = 4
    SAFT8kHz8BitStereo = 5
    SAFT8kHz16BitMono = 6
    SAFT8kHz16BitStereo = 7
    SAFT11kHz8BitMono = 8
    SAFT11kHz8BitStereo = 9
    SAFT11kHz16BitMono = 10
    SAFT11kHz16BitStereo = 11
    SAFT12kHz8BitMono = 12
    SAFT12kHz8BitStereo = 13
    SAFT12kHz16BitMono = 14
    SAFT12kHz16BitStereo = 15
    SAFT16kHz8BitMono = 16
    SAFT16kHz8BitStereo = 17
    SAFT16kHz16BitMono = 18
    SAFT16kHz16BitStereo = 19
    SAFT22kHz8BitMono = 20
    SAFT22kHz8BitStereo = 21
    SAFT22kHz16BitMono = 22
    SAFT22kHz16BitStereo = 23
    SAFT24kHz8BitMono = 24
    SAFT24kHz8BitStereo = 25
    SAFT24kHz16BitMono = 26
    SAFT24kHz16BitStereo = 27
    SAFT32kHz8BitMono = 28
    SAFT32kHz8BitStereo = 29
    SAFT32kHz16BitMono = 30
    SAFT32kHz16BitStereo = 31
    SAFT44kHz8BitMono = 32
    SAFT44kHz8BitStereo = 33
    SAFT44kHz16BitMono = 34
    SAFT44kHz16BitStereo = 35
    SAFT48kHz8BitMono = 36
    SAFT48kHz8BitStereo = 37
    SAFT48kHz16BitMono = 38
    SAFT48kHz16BitStereo = 39
    SAFTTrueSpeech_8kHz1BitMono = 40
    SAFTCCITT_ALaw_8kHzMono = 41
    SAFTCCITT_ALaw_8kHzStereo = 42
    SAFTCCITT_ALaw_11kHzMono = 43
    SAFTCCITT_ALaw_11kHzStereo = 44
    SAFTCCITT_ALaw_22kHzMono = 45
    SAFTCCITT_ALaw_22kHzStereo = 46
    SAFTCCITT_ALaw_44kHzMono = 47
    SAFTCCITT_ALaw_44kHzStereo = 48
    SAFTCCITT_uLaw_8kHzMono = 49
    SAFTCCITT_uLaw_8kHzStereo = 50
    SAFTCCITT_uLaw_11kHzMono = 51
    SAFTCCITT_uLaw_11kHzStereo = 52
    SAFTCCITT_uLaw_22kHzMono = 53
    SAFTCCITT_uLaw_22kHzStereo = 54
    SAFTCCITT_uLaw_44kHzMono = 55
    SAFTCCITT_uLaw_44kHzStereo = 56
    SAFTADPCM_8kHzMono = 57
    SAFTADPCM_8kHzStereo = 58
    SAFTADPCM_11kHzMono = 59
    SAFTADPCM_11kHzStereo = 60
    SAFTADPCM_22kHzMono = 61
    SAFTADPCM_22kHzStereo = 62
    SAFTADPCM_44kHzMono = 63
    SAFTADPCM_44kHzStereo = 64
    SAFTGSM610_8kHzMono = 65
    SAFTGSM610_11kHzMono = 66
    SAFTGSM610_22kHzMono = 67
    SAFTGSM610_44kHzMono = 68


class SpeechRecoEvents(IntFlag):
    SREStreamEnd = 1
    SRESoundStart = 2
    SRESoundEnd = 4
    SREPhraseStart = 8
    SRERecognition = 16
    SREHypothesis = 32
    SREBookmark = 64
    SREPropertyNumChange = 128
    SREPropertyStringChange = 256
    SREFalseRecognition = 512
    SREInterference = 1024
    SRERequestUI = 2048
    SREStateChange = 4096
    SREAdaptation = 8192
    SREStreamStart = 16384
    SRERecoOtherContext = 32768
    SREAudioLevel = 65536
    SREPrivate = 262144
    SREAllEvents = 393215


class SpeechRecoContextState(IntFlag):
    SRCS_Disabled = 0
    SRCS_Enabled = 1


class _SPAUDIOSTATE(IntFlag):
    SPAS_CLOSED = 0
    SPAS_STOP = 1
    SPAS_PAUSE = 2
    SPAS_RUN = 3


class DISPID_SpeechGrammarRule(IntFlag):
    DISPID_SGRAttributes = 1
    DISPID_SGRInitialState = 2
    DISPID_SGRName = 3
    DISPID_SGRId = 4
    DISPID_SGRClear = 5
    DISPID_SGRAddResource = 6
    DISPID_SGRAddState = 7


class DISPID_SpeechGrammarRules(IntFlag):
    DISPID_SGRsCount = 1
    DISPID_SGRsDynamic = 2
    DISPID_SGRsAdd = 3
    DISPID_SGRsCommit = 4
    DISPID_SGRsCommitAndSave = 5
    DISPID_SGRsFindRule = 6
    DISPID_SGRsItem = 0
    DISPID_SGRs_NewEnum = -4


class DISPID_SpeechGrammarRuleState(IntFlag):
    DISPID_SGRSRule = 1
    DISPID_SGRSTransitions = 2
    DISPID_SGRSAddWordTransition = 3
    DISPID_SGRSAddRuleTransition = 4
    DISPID_SGRSAddSpecialTransition = 5


class DISPID_SpeechGrammarRuleStateTransitions(IntFlag):
    DISPID_SGRSTsCount = 1
    DISPID_SGRSTsItem = 0
    DISPID_SGRSTs_NewEnum = -4


class DISPID_SpeechGrammarRuleStateTransition(IntFlag):
    DISPID_SGRSTType = 1
    DISPID_SGRSTText = 2
    DISPID_SGRSTRule = 3
    DISPID_SGRSTWeight = 4
    DISPID_SGRSTPropertyName = 5
    DISPID_SGRSTPropertyId = 6
    DISPID_SGRSTPropertyValue = 7
    DISPID_SGRSTNextState = 8


class DISPIDSPTSI(IntFlag):
    DISPIDSPTSI_ActiveOffset = 1
    DISPIDSPTSI_ActiveLength = 2
    DISPIDSPTSI_SelectionOffset = 3
    DISPIDSPTSI_SelectionLength = 4


class DISPID_SpeechRecoResult(IntFlag):
    DISPID_SRRRecoContext = 1
    DISPID_SRRTimes = 2
    DISPID_SRRAudioFormat = 3
    DISPID_SRRPhraseInfo = 4
    DISPID_SRRAlternates = 5
    DISPID_SRRAudio = 6
    DISPID_SRRSpeakAudio = 7
    DISPID_SRRSaveToMemory = 8
    DISPID_SRRDiscardResultInfo = 9


class DISPID_SpeechXMLRecoResult(IntFlag):
    DISPID_SRRGetXMLResult = 10
    DISPID_SRRGetXMLErrorInfo = 11


class DISPID_SpeechRecoResult2(IntFlag):
    DISPID_SRRSetTextFeedback = 12


class DISPID_SpeechPhraseBuilder(IntFlag):
    DISPID_SPPBRestorePhraseFromMemory = 1


class DISPID_SpeechRecoResultTimes(IntFlag):
    DISPID_SRRTStreamTime = 1
    DISPID_SRRTLength = 2
    DISPID_SRRTTickCount = 3
    DISPID_SRRTOffsetFromStart = 4


class SPVISEMES(IntFlag):
    SP_VISEME_0 = 0
    SP_VISEME_1 = 1
    SP_VISEME_2 = 2
    SP_VISEME_3 = 3
    SP_VISEME_4 = 4
    SP_VISEME_5 = 5
    SP_VISEME_6 = 6
    SP_VISEME_7 = 7
    SP_VISEME_8 = 8
    SP_VISEME_9 = 9
    SP_VISEME_10 = 10
    SP_VISEME_11 = 11
    SP_VISEME_12 = 12
    SP_VISEME_13 = 13
    SP_VISEME_14 = 14
    SP_VISEME_15 = 15
    SP_VISEME_16 = 16
    SP_VISEME_17 = 17
    SP_VISEME_18 = 18
    SP_VISEME_19 = 19
    SP_VISEME_20 = 20
    SP_VISEME_21 = 21


class DISPID_SpeechPhraseAlternate(IntFlag):
    DISPID_SPARecoResult = 1
    DISPID_SPAStartElementInResult = 2
    DISPID_SPANumberOfElementsInResult = 3
    DISPID_SPAPhraseInfo = 4
    DISPID_SPACommit = 5


class SPWAVEFORMATTYPE(IntFlag):
    SPWF_INPUT = 0
    SPWF_SRENGINE = 1


class DISPID_SpeechPhraseAlternates(IntFlag):
    DISPID_SPAsCount = 1
    DISPID_SPAsItem = 0
    DISPID_SPAs_NewEnum = -4


class SPFILEMODE(IntFlag):
    SPFM_OPEN_READONLY = 0
    SPFM_OPEN_READWRITE = 1
    SPFM_CREATE = 2
    SPFM_CREATE_ALWAYS = 3
    SPFM_NUM_MODES = 4


class DISPID_SpeechPhraseInfo(IntFlag):
    DISPID_SPILanguageId = 1
    DISPID_SPIGrammarId = 2
    DISPID_SPIStartTime = 3
    DISPID_SPIAudioStreamPosition = 4
    DISPID_SPIAudioSizeBytes = 5
    DISPID_SPIRetainedSizeBytes = 6
    DISPID_SPIAudioSizeTime = 7
    DISPID_SPIRule = 8
    DISPID_SPIProperties = 9
    DISPID_SPIElements = 10
    DISPID_SPIReplacements = 11
    DISPID_SPIEngineId = 12
    DISPID_SPIEnginePrivateData = 13
    DISPID_SPISaveToMemory = 14
    DISPID_SPIGetText = 15
    DISPID_SPIGetDisplayAttributes = 16


class DISPID_SpeechPhraseElement(IntFlag):
    DISPID_SPEAudioTimeOffset = 1
    DISPID_SPEAudioSizeTime = 2
    DISPID_SPEAudioStreamOffset = 3
    DISPID_SPEAudioSizeBytes = 4
    DISPID_SPERetainedStreamOffset = 5
    DISPID_SPERetainedSizeBytes = 6
    DISPID_SPEDisplayText = 7
    DISPID_SPELexicalForm = 8
    DISPID_SPEPronunciation = 9
    DISPID_SPEDisplayAttributes = 10
    DISPID_SPERequiredConfidence = 11
    DISPID_SPEActualConfidence = 12
    DISPID_SPEEngineConfidence = 13


class DISPID_SpeechPhraseElements(IntFlag):
    DISPID_SPEsCount = 1
    DISPID_SPEsItem = 0
    DISPID_SPEs_NewEnum = -4


class DISPID_SpeechPhraseReplacement(IntFlag):
    DISPID_SPRDisplayAttributes = 1
    DISPID_SPRText = 2
    DISPID_SPRFirstElement = 3
    DISPID_SPRNumberOfElements = 4


class DISPID_SpeechPhraseReplacements(IntFlag):
    DISPID_SPRsCount = 1
    DISPID_SPRsItem = 0
    DISPID_SPRs_NewEnum = -4


class SPVPRIORITY(IntFlag):
    SPVPRI_NORMAL = 0
    SPVPRI_ALERT = 1
    SPVPRI_OVER = 2


class SPEVENTENUM(IntFlag):
    SPEI_UNDEFINED = 0
    SPEI_START_INPUT_STREAM = 1
    SPEI_END_INPUT_STREAM = 2
    SPEI_VOICE_CHANGE = 3
    SPEI_TTS_BOOKMARK = 4
    SPEI_WORD_BOUNDARY = 5
    SPEI_PHONEME = 6
    SPEI_SENTENCE_BOUNDARY = 7
    SPEI_VISEME = 8
    SPEI_TTS_AUDIO_LEVEL = 9
    SPEI_TTS_PRIVATE = 15
    SPEI_MIN_TTS = 1
    SPEI_MAX_TTS = 15
    SPEI_END_SR_STREAM = 34
    SPEI_SOUND_START = 35
    SPEI_SOUND_END = 36
    SPEI_PHRASE_START = 37
    SPEI_RECOGNITION = 38
    SPEI_HYPOTHESIS = 39
    SPEI_SR_BOOKMARK = 40
    SPEI_PROPERTY_NUM_CHANGE = 41
    SPEI_PROPERTY_STRING_CHANGE = 42
    SPEI_FALSE_RECOGNITION = 43
    SPEI_INTERFERENCE = 44
    SPEI_REQUEST_UI = 45
    SPEI_RECO_STATE_CHANGE = 46
    SPEI_ADAPTATION = 47
    SPEI_START_SR_STREAM = 48
    SPEI_RECO_OTHER_CONTEXT = 49
    SPEI_SR_AUDIO_LEVEL = 50
    SPEI_SR_RETAINEDAUDIO = 51
    SPEI_SR_PRIVATE = 52
    SPEI_ACTIVE_CATEGORY_CHANGED = 53
    SPEI_RESERVED5 = 54
    SPEI_RESERVED6 = 55
    SPEI_MIN_SR = 34
    SPEI_MAX_SR = 55
    SPEI_RESERVED1 = 30
    SPEI_RESERVED2 = 33
    SPEI_RESERVED3 = 63


class DISPID_SpeechPhraseProperty(IntFlag):
    DISPID_SPPName = 1
    DISPID_SPPId = 2
    DISPID_SPPValue = 3
    DISPID_SPPFirstElement = 4
    DISPID_SPPNumberOfElements = 5
    DISPID_SPPEngineConfidence = 6
    DISPID_SPPConfidence = 7
    DISPID_SPPParent = 8
    DISPID_SPPChildren = 9


class DISPID_SpeechPhraseProperties(IntFlag):
    DISPID_SPPsCount = 1
    DISPID_SPPsItem = 0
    DISPID_SPPs_NewEnum = -4


class DISPID_SpeechPhraseRule(IntFlag):
    DISPID_SPRuleName = 1
    DISPID_SPRuleId = 2
    DISPID_SPRuleFirstElement = 3
    DISPID_SPRuleNumberOfElements = 4
    DISPID_SPRuleParent = 5
    DISPID_SPRuleChildren = 6
    DISPID_SPRuleConfidence = 7
    DISPID_SPRuleEngineConfidence = 8


class DISPID_SpeechPhraseRules(IntFlag):
    DISPID_SPRulesCount = 1
    DISPID_SPRulesItem = 0
    DISPID_SPRules_NewEnum = -4


class DISPID_SpeechLexicon(IntFlag):
    DISPID_SLGenerationId = 1
    DISPID_SLGetWords = 2
    DISPID_SLAddPronunciation = 3
    DISPID_SLAddPronunciationByPhoneIds = 4
    DISPID_SLRemovePronunciation = 5
    DISPID_SLRemovePronunciationByPhoneIds = 6
    DISPID_SLGetPronunciations = 7
    DISPID_SLGetGenerationChange = 8


class DISPID_SpeechLexiconWords(IntFlag):
    DISPID_SLWsCount = 1
    DISPID_SLWsItem = 0
    DISPID_SLWs_NewEnum = -4


class DISPID_SpeechLexiconWord(IntFlag):
    DISPID_SLWLangId = 1
    DISPID_SLWType = 2
    DISPID_SLWWord = 3
    DISPID_SLWPronunciations = 4


class DISPID_SpeechLexiconProns(IntFlag):
    DISPID_SLPsCount = 1
    DISPID_SLPsItem = 0
    DISPID_SLPs_NewEnum = -4


class DISPID_SpeechLexiconPronunciation(IntFlag):
    DISPID_SLPType = 1
    DISPID_SLPLangId = 2
    DISPID_SLPPartOfSpeech = 3
    DISPID_SLPPhoneIds = 4
    DISPID_SLPSymbolic = 5


class DISPID_SpeechPhoneConverter(IntFlag):
    DISPID_SPCLangId = 1
    DISPID_SPCPhoneToId = 2
    DISPID_SPCIdToPhone = 3


class SPDATAKEYLOCATION(IntFlag):
    SPDKL_DefaultLocation = 0
    SPDKL_CurrentUser = 1
    SPDKL_LocalMachine = 2
    SPDKL_CurrentConfig = 5


class SPRECOSTATE(IntFlag):
    SPRST_INACTIVE = 0
    SPRST_ACTIVE = 1
    SPRST_ACTIVE_ALWAYS = 2
    SPRST_INACTIVE_WITH_PURGE = 3
    SPRST_NUM_STATES = 4


class SpeechVoiceSpeakFlags(IntFlag):
    SVSFDefault = 0
    SVSFlagsAsync = 1
    SVSFPurgeBeforeSpeak = 2
    SVSFIsFilename = 4
    SVSFIsXML = 8
    SVSFIsNotXML = 16
    SVSFPersistXML = 32
    SVSFNLPSpeakPunc = 64
    SVSFParseSapi = 128
    SVSFParseSsml = 256
    SVSFParseAutodetect = 0
    SVSFNLPMask = 64
    SVSFParseMask = 384
    SVSFVoiceMask = 511
    SVSFUnusedFlags = -512


class SpeechDiscardType(IntFlag):
    SDTProperty = 1
    SDTReplacement = 2
    SDTRule = 4
    SDTDisplayText = 8
    SDTLexicalForm = 16
    SDTPronunciation = 32
    SDTAudio = 64
    SDTAlternates = 128
    SDTAll = 255


class SpeechWordType(IntFlag):
    SWTAdded = 1
    SWTDeleted = 2


class DISPID_SpeechDataKey(IntFlag):
    DISPID_SDKSetBinaryValue = 1
    DISPID_SDKGetBinaryValue = 2
    DISPID_SDKSetStringValue = 3
    DISPID_SDKGetStringValue = 4
    DISPID_SDKSetLongValue = 5
    DISPID_SDKGetlongValue = 6
    DISPID_SDKOpenKey = 7
    DISPID_SDKCreateKey = 8
    DISPID_SDKDeleteKey = 9
    DISPID_SDKDeleteValue = 10
    DISPID_SDKEnumKeys = 11
    DISPID_SDKEnumValues = 12


class DISPID_SpeechObjectToken(IntFlag):
    DISPID_SOTId = 1
    DISPID_SOTDataKey = 2
    DISPID_SOTCategory = 3
    DISPID_SOTGetDescription = 4
    DISPID_SOTSetId = 5
    DISPID_SOTGetAttribute = 6
    DISPID_SOTCreateInstance = 7
    DISPID_SOTRemove = 8
    DISPID_SOTGetStorageFileName = 9
    DISPID_SOTRemoveStorageFileName = 10
    DISPID_SOTIsUISupported = 11
    DISPID_SOTDisplayUI = 12
    DISPID_SOTMatchesAttributes = 13


class DISPID_SpeechObjectTokens(IntFlag):
    DISPID_SOTsCount = 1
    DISPID_SOTsItem = 0
    DISPID_SOTs_NewEnum = -4


class DISPID_SpeechObjectTokenCategory(IntFlag):
    DISPID_SOTCId = 1
    DISPID_SOTCDefault = 2
    DISPID_SOTCSetId = 3
    DISPID_SOTCGetDataKey = 4
    DISPID_SOTCEnumerateTokens = 5


class DISPID_SpeechAudioFormat(IntFlag):
    DISPID_SAFType = 1
    DISPID_SAFGuid = 2
    DISPID_SAFGetWaveFormatEx = 3
    DISPID_SAFSetWaveFormatEx = 4


class DISPID_SpeechBaseStream(IntFlag):
    DISPID_SBSFormat = 1
    DISPID_SBSRead = 2
    DISPID_SBSWrite = 3
    DISPID_SBSSeek = 4


class DISPID_SpeechAudio(IntFlag):
    DISPID_SAStatus = 200
    DISPID_SABufferInfo = 201
    DISPID_SADefaultFormat = 202
    DISPID_SAVolume = 203
    DISPID_SABufferNotifySize = 204
    DISPID_SAEventHandle = 205
    DISPID_SASetState = 206


class DISPID_SpeechMMSysAudio(IntFlag):
    DISPID_SMSADeviceId = 300
    DISPID_SMSALineId = 301
    DISPID_SMSAMMHandle = 302


class DISPID_SpeechFileStream(IntFlag):
    DISPID_SFSOpen = 100
    DISPID_SFSClose = 101


class DISPID_SpeechCustomStream(IntFlag):
    DISPID_SCSBaseStream = 100


class DISPID_SpeechMemoryStream(IntFlag):
    DISPID_SMSSetData = 100
    DISPID_SMSGetData = 101


class DISPID_SpeechAudioStatus(IntFlag):
    DISPID_SASFreeBufferSpace = 1
    DISPID_SASNonBlockingIO = 2
    DISPID_SASState = 3
    DISPID_SASCurrentSeekPosition = 4
    DISPID_SASCurrentDevicePosition = 5


class DISPID_SpeechAudioBufferInfo(IntFlag):
    DISPID_SABIMinNotification = 1
    DISPID_SABIBufferSize = 2
    DISPID_SABIEventBias = 3


class DISPID_SpeechWaveFormatEx(IntFlag):
    DISPID_SWFEFormatTag = 1
    DISPID_SWFEChannels = 2
    DISPID_SWFESamplesPerSec = 3
    DISPID_SWFEAvgBytesPerSec = 4
    DISPID_SWFEBlockAlign = 5
    DISPID_SWFEBitsPerSample = 6
    DISPID_SWFEExtraData = 7


class DISPID_SpeechVoice(IntFlag):
    DISPID_SVStatus = 1
    DISPID_SVVoice = 2
    DISPID_SVAudioOutput = 3
    DISPID_SVAudioOutputStream = 4
    DISPID_SVRate = 5
    DISPID_SVVolume = 6
    DISPID_SVAllowAudioOuputFormatChangesOnNextSet = 7
    DISPID_SVEventInterests = 8
    DISPID_SVPriority = 9
    DISPID_SVAlertBoundary = 10
    DISPID_SVSyncronousSpeakTimeout = 11
    DISPID_SVSpeak = 12
    DISPID_SVSpeakStream = 13
    DISPID_SVPause = 14
    DISPID_SVResume = 15
    DISPID_SVSkip = 16
    DISPID_SVGetVoices = 17
    DISPID_SVGetAudioOutputs = 18
    DISPID_SVWaitUntilDone = 19
    DISPID_SVSpeakCompleteEvent = 20
    DISPID_SVIsUISupported = 21
    DISPID_SVDisplayUI = 22


class DISPID_SpeechVoiceStatus(IntFlag):
    DISPID_SVSCurrentStreamNumber = 1
    DISPID_SVSLastStreamNumberQueued = 2
    DISPID_SVSLastResult = 3
    DISPID_SVSRunningState = 4
    DISPID_SVSInputWordPosition = 5
    DISPID_SVSInputWordLength = 6
    DISPID_SVSInputSentencePosition = 7
    DISPID_SVSInputSentenceLength = 8
    DISPID_SVSLastBookmark = 9
    DISPID_SVSLastBookmarkId = 10
    DISPID_SVSPhonemeId = 11
    DISPID_SVSVisemeId = 12


class DISPID_SpeechVoiceEvent(IntFlag):
    DISPID_SVEStreamStart = 1
    DISPID_SVEStreamEnd = 2
    DISPID_SVEVoiceChange = 3
    DISPID_SVEBookmark = 4
    DISPID_SVEWord = 5
    DISPID_SVEPhoneme = 6
    DISPID_SVESentenceBoundary = 7
    DISPID_SVEViseme = 8
    DISPID_SVEAudioLevel = 9
    DISPID_SVEEnginePrivate = 10


class DISPID_SpeechRecognizer(IntFlag):
    DISPID_SRRecognizer = 1
    DISPID_SRAllowAudioInputFormatChangesOnNextSet = 2
    DISPID_SRAudioInput = 3
    DISPID_SRAudioInputStream = 4
    DISPID_SRIsShared = 5
    DISPID_SRState = 6
    DISPID_SRStatus = 7
    DISPID_SRProfile = 8
    DISPID_SREmulateRecognition = 9
    DISPID_SRCreateRecoContext = 10
    DISPID_SRGetFormat = 11
    DISPID_SRSetPropertyNumber = 12
    DISPID_SRGetPropertyNumber = 13
    DISPID_SRSetPropertyString = 14
    DISPID_SRGetPropertyString = 15
    DISPID_SRIsUISupported = 16
    DISPID_SRDisplayUI = 17
    DISPID_SRGetRecognizers = 18
    DISPID_SVGetAudioInputs = 19
    DISPID_SVGetProfiles = 20


class SpeechEmulationCompareFlags(IntFlag):
    SECFIgnoreCase = 1
    SECFIgnoreKanaType = 65536
    SECFIgnoreWidth = 131072
    SECFNoSpecialChars = 536870912
    SECFEmulateResult = 1073741824
    SECFDefault = 196609


class DISPID_SpeechRecognizerStatus(IntFlag):
    DISPID_SRSAudioStatus = 1
    DISPID_SRSCurrentStreamPosition = 2
    DISPID_SRSCurrentStreamNumber = 3
    DISPID_SRSNumberOfActiveRules = 4
    DISPID_SRSClsidEngine = 5
    DISPID_SRSSupportedLanguages = 6


class DISPID_SpeechRecoContext(IntFlag):
    DISPID_SRCRecognizer = 1
    DISPID_SRCAudioInInterferenceStatus = 2
    DISPID_SRCRequestedUIType = 3
    DISPID_SRCVoice = 4
    DISPID_SRAllowVoiceFormatMatchingOnNextSet = 5
    DISPID_SRCVoicePurgeEvent = 6
    DISPID_SRCEventInterests = 7
    DISPID_SRCCmdMaxAlternates = 8
    DISPID_SRCState = 9
    DISPID_SRCRetainedAudio = 10
    DISPID_SRCRetainedAudioFormat = 11
    DISPID_SRCPause = 12
    DISPID_SRCResume = 13
    DISPID_SRCCreateGrammar = 14
    DISPID_SRCCreateResultFromMemory = 15
    DISPID_SRCBookmark = 16
    DISPID_SRCSetAdaptationData = 17


class DISPIDSPRG(IntFlag):
    DISPID_SRGId = 1
    DISPID_SRGRecoContext = 2
    DISPID_SRGState = 3
    DISPID_SRGRules = 4
    DISPID_SRGReset = 5
    DISPID_SRGCommit = 6
    DISPID_SRGCmdLoadFromFile = 7
    DISPID_SRGCmdLoadFromObject = 8
    DISPID_SRGCmdLoadFromResource = 9
    DISPID_SRGCmdLoadFromMemory = 10
    DISPID_SRGCmdLoadFromProprietaryGrammar = 11
    DISPID_SRGCmdSetRuleState = 12
    DISPID_SRGCmdSetRuleIdState = 13
    DISPID_SRGDictationLoad = 14
    DISPID_SRGDictationUnload = 15
    DISPID_SRGDictationSetState = 16
    DISPID_SRGSetWordSequenceData = 17
    DISPID_SRGSetTextSelection = 18
    DISPID_SRGIsPronounceable = 19


class DISPID_SpeechRecoContextEvents(IntFlag):
    DISPID_SRCEStartStream = 1
    DISPID_SRCEEndStream = 2
    DISPID_SRCEBookmark = 3
    DISPID_SRCESoundStart = 4
    DISPID_SRCESoundEnd = 5
    DISPID_SRCEPhraseStart = 6
    DISPID_SRCERecognition = 7
    DISPID_SRCEHypothesis = 8
    DISPID_SRCEPropertyNumberChange = 9
    DISPID_SRCEPropertyStringChange = 10
    DISPID_SRCEFalseRecognition = 11
    DISPID_SRCEInterference = 12
    DISPID_SRCERequestUI = 13
    DISPID_SRCERecognizerStateChange = 14
    DISPID_SRCEAdaptation = 15
    DISPID_SRCERecognitionForOtherContext = 16
    DISPID_SRCEAudioLevel = 17
    DISPID_SRCEEnginePrivate = 18


SPAUDIOSTATE = _SPAUDIOSTATE
SPSTREAMFORMATTYPE = SPWAVEFORMATTYPE


__all__ = [
    'SITooFast', 'DISPID_SAFSetWaveFormatEx', 'DISPID_SpeechLexicon',
    'SAFT12kHz8BitMono', 'DISPID_SRRTTickCount',
    'DISPID_SRGCmdLoadFromMemory', 'SPSHORTCUTPAIRLIST',
    'DISPID_SPEEngineConfidence', 'SGDSActive',
    'SPINTERFERENCE_TOOQUIET', 'ISpeechCustomStream',
    'Speech_Default_Weight', 'SRERecognition',
    'DISPID_SpeechMemoryStream', 'ISpeechRecoGrammar',
    'SAFTCCITT_uLaw_11kHzMono', 'DISPID_SDKGetStringValue',
    'SP_VISEME_0', 'DISPID_SRSAudioStatus', 'ISpeechMemoryStream',
    'SpeechRegistryLocalMachineRoot', 'SSFMCreateForWrite',
    'SGDSActiveUserDelimited', 'DISPID_SVVolume',
    'SPSMF_SRGS_SAPIPROPERTIES', 'DISPID_SGRsDynamic',
    'eLEXTYPE_USER', 'SECFIgnoreCase', 'SAFT11kHz16BitStereo',
    'DISPID_SRRGetXMLErrorInfo', 'DISPID_SAStatus',
    'SAFT16kHz16BitStereo', 'DISPID_SpeechObjectToken',
    'SPRST_ACTIVE', 'SITooLoud', 'SPSHORTCUTPAIR', 'SGRSTTDictation',
    'SREPropertyStringChange', 'SECNormalConfidence',
    'DISPID_SPEDisplayAttributes', 'SPDKL_DefaultLocation',
    'SPBOOKMARKOPTIONS', 'SBONone', 'SP_VISEME_21',
    'SpeechRuleAttributes', 'SPRULE', 'DISPID_SpeechAudioStatus',
    'DISPID_SVSpeakCompleteEvent', 'SECFIgnoreWidth',
    'DISPID_SRCEHypothesis', 'ISpeechLexiconPronunciations',
    'DISPID_SPIElements', 'DISPID_SpeechLexiconProns',
    'DISPID_SRAllowAudioInputFormatChangesOnNextSet',
    'DISPID_SRSCurrentStreamNumber', 'DISPID_SVESentenceBoundary',
    'SpeechCategoryAudioOut', 'DISPID_SGRSTsCount',
    'tagSPPROPERTYINFO', 'DISPID_SRGCmdLoadFromFile',
    'SDA_Two_Trailing_Spaces', 'SpNotifyTranslator',
    'DISPID_SPAs_NewEnum', 'SpNullPhoneConverter',
    'SRERecoOtherContext', 'ISpProperties', 'SPEI_TTS_BOOKMARK',
    'SASPause', 'DISPID_SpeechCustomStream', 'SPVPRI_NORMAL',
    'ISpRecoContext', 'ISpNotifyTranslator',
    'SPEI_PROPERTY_STRING_CHANGE', 'DISPID_SVWaitUntilDone',
    'DISPID_SGRsCommitAndSave', 'SPPHRASEPROPERTY',
    'ISpeechMMSysAudio', 'SPWT_DISPLAY', 'SWTDeleted',
    'SAFTADPCM_8kHzMono', 'SGLexicalNoSpecialChars', 'SPAR_Medium',
    'DISPID_SGRsItem', 'SSFMOpenForRead', 'DISPID_SGRSTs_NewEnum',
    'ISpeechAudioFormat', 'SPEI_UNDEFINED', 'SPRST_NUM_STATES',
    'SGRSTTWildcard', 'SPRS_INACTIVE', 'SDTAlternates',
    'ISpeechPhraseInfo', 'DISPID_SPEAudioStreamOffset',
    'DISPID_SPIReplacements', 'DISPID_SRGDictationSetState',
    'DISPID_SPEDisplayText', 'SVESentenceBoundary',
    'DISPID_SRGDictationUnload', 'SPAUDIOBUFFERINFO',
    'SDKLLocalMachine', 'ISpeechGrammarRuleStateTransitions',
    'SPEI_SR_BOOKMARK', 'SREStateChange', 'DISPID_SRRPhraseInfo',
    'SPWT_LEXICAL_NO_SPECIAL_CHARS', 'DISPID_SpeechRecoResult2',
    'SAFTCCITT_ALaw_8kHzStereo', 'SPPS_RESERVED2', 'SGSEnabled',
    'SREPrivate', 'DISPID_SPRFirstElement', 'SpeechMicTraining',
    'DISPID_SGRSTWeight', 'SVPNormal', 'DISPID_SPARecoResult',
    'DISPID_SVPriority', 'DISPIDSPTSI_SelectionOffset',
    'DISPID_SPPsCount', 'SLOStatic', 'SP_VISEME_5',
    'DISPID_SGRSTransitions', 'DISPID_SRIsUISupported',
    'DISPID_SVSInputWordPosition', 'SpeechTokenValueCLSID', 'SVP_7',
    'SPWF_INPUT', 'SVP_9', 'SpeechRegistryUserRoot', 'SpCustomStream',
    'SPPS_Unknown', 'SAFTCCITT_ALaw_8kHzMono',
    'DISPID_SPPFirstElement', 'SRSEIsSpeaking', 'SDTReplacement',
    'DISPID_SBSSeek', 'DISPID_SAEventHandle',
    'SGDSActiveWithAutoPause', 'SVP_20',
    'SpeechGrammarRuleStateTransitionType', 'SPEI_TTS_AUDIO_LEVEL',
    'DISPID_SLGetWords', 'tagSTATSTG',
    '__MIDL___MIDL_itf_sapi_0000_0020_0001', 'SVP_1',
    'SSSPTRelativeToCurrentPosition', 'SPBO_TIME_UNITS',
    'SPINTERFERENCE_LATENCY_WARNING', 'SAFT32kHz16BitMono',
    'ISpVoice', 'DISPID_SLGenerationId', 'SASClosed',
    'DISPID_SPPParent', 'DISPID_SpeechLexiconWord', 'IEnumString',
    'DISPID_SPERetainedStreamOffset',
    'DISPID_SpeechObjectTokenCategory', 'DISPID_SGRSRule',
    'SpeechAudioFormatGUIDWave', 'SPLO_DYNAMIC', 'SVSFParseSsml',
    'SpeechAudioFormatType', 'eLEXTYPE_PRIVATE3', 'SGRSTTWord',
    'eLEXTYPE_MORPHOLOGY', 'DISPID_SPPValue', 'SpStream',
    'SAFTADPCM_22kHzMono', 'SPXMLRESULTOPTIONS', 'SPGS_DISABLED',
    'SPPS_Verb', 'DISPID_SPRNumberOfElements', 'DISPID_SDKCreateKey',
    'SVP_6', 'ISpGrammarBuilder', 'DISPID_SRGCmdLoadFromResource',
    'SREPropertyNumChange', 'SPPS_RESERVED1', 'ISpeechAudio',
    'SAFT48kHz16BitMono', 'DISPID_SVEViseme', 'SPWT_PRONUNCIATION',
    'eLEXTYPE_PRIVATE15', 'DISPID_SVSVisemeId', 'STSF_FlagCreate',
    'DISPID_SRGDictationLoad', 'SPRECOSTATE',
    'SpeechPropertyNormalConfidenceThreshold',
    'SPEI_END_INPUT_STREAM', 'DISPID_SGRsFindRule',
    'SAFTCCITT_uLaw_44kHzStereo', 'DISPID_SLPPhoneIds', 'SDTRule',
    'ISpeechDataKey', 'DISPID_SASState', 'SP_VISEME_15',
    'DISPID_SPCLangId', 'ISpRecoResult', 'ISpeechObjectTokens',
    'DISPID_SFSClose', 'eLEXTYPE_PRIVATE4', 'SpeechDiscardType',
    'SREAllEvents', 'SDKLDefaultLocation', 'DISPID_SPRulesCount',
    'SPEI_PROPERTY_NUM_CHANGE', 'DISPID_SOTDisplayUI', 'SP_VISEME_4',
    'eLEXTYPE_PRIVATE1', 'ISpPhraseAlt', 'SAFTExtendedAudioFormat',
    'SpeechRecoContextState', 'SpeechTokenKeyAttributes',
    'DISPID_SPRsCount', 'DISPID_SWFEExtraData', 'SPEI_RESERVED2',
    'DISPID_SVSInputSentenceLength', 'DISPID_SPERequiredConfidence',
    'DISPID_SRIsShared', 'DISPID_SPIProperties', 'SPPS_RESERVED3',
    'SPRS_ACTIVE_USER_DELIMITED', 'SVSFNLPSpeakPunc',
    'DISPID_SOTCSetId', 'eLEXTYPE_VENDORLEXICON',
    'DISPID_SPEAudioSizeTime', 'SVSFPersistXML', 'SVP_12',
    'SpeechPartOfSpeech', 'SpeechPropertyLowConfidenceThreshold',
    'SPAUDIOSTATE', 'DISPID_SVAllowAudioOuputFormatChangesOnNextSet',
    'SP_VISEME_2', 'SPEI_MAX_TTS', 'ISpeechGrammarRule',
    'SPWORDPRONUNCIATION', 'ISpeechObjectToken',
    'DISPID_SpeechPhraseAlternate', 'DISPID_SGRs_NewEnum',
    'DISPID_SABIEventBias', 'DISPID_SpeechRecoContext', 'SPWORDLIST',
    'SPRULESTATE', 'SAFTText', 'SAFT11kHz8BitMono',
    'eLEXTYPE_RESERVED9', 'SPSInterjection', 'DISPID_SABufferInfo',
    'SpeechVoicePriority', 'DISPID_SVEBookmark', 'SPPS_NotOverriden',
    'DISPID_SAFType', 'SPEI_SOUND_START', 'ISpObjectTokenCategory',
    'SpeechVisemeType', 'DISPID_SRCRequestedUIType',
    'DISPID_SVEventInterests', 'DISPID_SRGetRecognizers',
    'ISpShortcut', 'SPEI_SR_RETAINEDAUDIO', 'DISPID_SVSpeakStream',
    'SPEI_PHRASE_START', 'tagSPTEXTSELECTIONINFO',
    'SVEEndInputStream', 'SRSInactiveWithPurge', 'ISpeechRecoResult',
    'DISPID_SASCurrentSeekPosition', 'DISPID_SGRSTType',
    'DISPID_SRCERecognition', 'DISPID_SRAudioInputStream',
    'SPEI_RESERVED5', 'LONG_PTR', 'SVSFIsXML', 'DISPID_SWFEChannels',
    'SPBINARYGRAMMAR', 'SPWP_KNOWN_WORD_PRONOUNCEABLE',
    'SPINTERFERENCE_TOOSLOW', 'ISpeechFileStream',
    'DISPID_SPEActualConfidence', 'eLEXTYPE_PRIVATE13',
    'DISPID_SPEsCount', 'SVP_14', 'SBOPause', 'SVP_21',
    'DISPID_SVSpeak', 'DISPID_SRSClsidEngine', 'SVSFlagsAsync',
    'DISPID_SASFreeBufferSpace', 'DISPID_SPPs_NewEnum',
    'ISpNotifySource', 'DISPID_SDKDeleteValue',
    'DISPID_SpeechGrammarRuleState', 'ISpRecoGrammar',
    'DISPID_SVEAudioLevel', 'SPWORDTYPE', 'DISPID_SGRSTRule',
    'SAFTADPCM_22kHzStereo', 'SPEI_RESERVED1', 'SAFT11kHz16BitMono',
    'SREHypothesis', 'DISPID_SDKSetLongValue',
    'DISPID_SWFEAvgBytesPerSec', 'ISpPhoneConverter', 'SP_VISEME_13',
    'SPSEMANTICERRORINFO', 'DISPID_SOTIsUISupported',
    'DISPID_SVGetProfiles', 'SPDKL_CurrentConfig', 'SVF_None',
    'DISPID_SPPConfidence', 'SAFT11kHz8BitStereo',
    'DISPID_SOTCGetDataKey', 'SPSFunction', 'SpCompressedLexicon',
    'DISPID_SGRSTNextState', 'SPWT_LEXICAL', 'DISPID_SVStatus',
    'SPCATEGORYTYPE', 'SPEI_REQUEST_UI', 'DISPID_SRGState',
    'DISPID_SLPLangId', 'DISPID_SOTRemove', 'SPAS_RUN',
    'DISPID_SRStatus', 'SRERequestUI', 'DISPID_SRCPause',
    'DISPID_SAFGuid', 'DISPID_SVSLastStreamNumberQueued',
    'DISPID_SPEAudioSizeBytes', 'ISpeechPhraseReplacements',
    'SPPARTOFSPEECH', 'DISPID_SASNonBlockingIO',
    'DISPID_SVAudioOutput', 'DISPID_SRRSetTextFeedback',
    'SECLowConfidence', 'SGLexical', 'SpeechPropertyAdaptationOn',
    'DISPID_SVVoice', 'SINone', 'DISPID_SGRAddResource',
    'SpeechRunState', 'ISpeechBaseStream', 'SpInProcRecoContext',
    'SAFT8kHz8BitMono', 'SAFT48kHz8BitStereo', 'SPRECOCONTEXTSTATUS',
    'SDTProperty', 'ISpeechPhraseRules', 'SDTAll',
    'SPXRO_Alternates_SML', 'SPSMF_SRGS_SEMANTICINTERPRETATION_MS',
    'DISPID_SDKGetlongValue', 'DISPID_SpeechMMSysAudio',
    'DISPID_SpeechPhraseReplacement', 'SPEI_SR_PRIVATE',
    'DISPID_SCSBaseStream', 'SPRS_ACTIVE', 'DISPID_SMSADeviceId',
    'eLEXTYPE_RESERVED8', 'DISPID_SLAddPronunciation',
    'SpeechRecoProfileProperties', 'DISPID_SPRDisplayAttributes',
    'SSTTDictation', 'SPEI_TTS_PRIVATE', 'SpeechGrammarTagDictation',
    'DISPID_SRGRules', 'SPSTREAMFORMATTYPE', 'SGDSInactive',
    'SSTTTextBuffer', 'SPAO_RETAIN_AUDIO', 'SPSEMANTICFORMAT',
    'SDKLCurrentConfig', 'SAFTADPCM_11kHzStereo',
    'DISPIDSPTSI_ActiveLength', 'DISPID_SpeechPhraseProperty',
    'SAFT8kHz16BitStereo', 'SREFalseRecognition', 'SPVPRI_OVER',
    'SDA_No_Trailing_Space', 'SPPS_Modifier', 'SVP_5', 'ISpLexicon',
    'SVSFUnusedFlags', 'SAFTCCITT_uLaw_22kHzStereo',
    'SPPS_SuppressWord', 'SRATopLevel', 'SAFTCCITT_ALaw_22kHzMono',
    'SpeechAudioState', 'ISpeechObjectTokenCategory', 'SITooQuiet',
    'ISpStreamFormatConverter', 'ISpObjectToken', 'DISPID_SGRName',
    'ISpeechGrammarRuleState', 'SPPHRASEREPLACEMENT',
    'SpeechRuleState', 'DISPID_SGRInitialState', 'SP_VISEME_14',
    'DISPID_SPELexicalForm', 'DISPID_SRCESoundEnd', 'SLTUser',
    'SPDATAKEYLOCATION', 'SPWP_UNKNOWN_WORD_UNPRONOUNCEABLE',
    'SAFT44kHz16BitStereo', 'SDA_One_Trailing_Space',
    'DISPID_SPIAudioSizeTime', 'DISPID_SRCCreateGrammar',
    'DISPID_SpeechPhraseElements', 'DISPID_SVSLastBookmark',
    'SPEVENT', 'SVEVoiceChange', 'SAFT16kHz16BitMono',
    'DISPID_SOTDataKey', 'DISPID_SRGCommit',
    'DISPID_SpeechGrammarRules', 'SPCS_ENABLED',
    'SAFTADPCM_8kHzStereo', 'SpSharedRecoContext', 'SVSFDefault',
    'SASStop', 'DISPID_SGRSTText', 'SAFT16kHz8BitMono',
    'SREPhraseStart', 'DISPID_SGRSTPropertyId', 'SP_VISEME_9',
    'DISPID_SPRuleId', 'SAFTCCITT_ALaw_11kHzStereo',
    'DISPID_SRCCmdMaxAlternates', 'ISpeechVoiceStatus',
    'SVSFParseSapi', 'SP_VISEME_17', 'SPAUDIOOPTIONS', 'SVEViseme',
    'SPVPRI_ALERT', 'SPTEXTSELECTIONINFO',
    'DISPID_SABIMinNotification', 'SpInprocRecognizer',
    'SPWP_UNKNOWN_WORD_PRONOUNCEABLE', 'DISPIDSPRG', 'SVPOver',
    'SRAONone', 'SpeechCategoryAppLexicons',
    'DISPID_SOTMatchesAttributes', 'SpVoice', 'SVSFPurgeBeforeSpeak',
    'SFTInput', 'SAFT32kHz16BitStereo', 'DISPID_SOTCDefault',
    'DISPID_SRCEPropertyStringChange', 'DISPID_SLPType',
    'SRTAutopause', 'SAFTNoAssignedFormat',
    'SpeechTokenIdUserLexicon', 'SSTTWildcard', 'SPPROPERTYINFO',
    'DISPID_SOTCreateInstance', 'ISpDataKey', 'SRADefaultToActive',
    'ISpAudio', 'DISPID_SRCERecognizerStateChange',
    '__MIDL___MIDL_itf_sapi_0000_0020_0002', 'SpeechLoadOption',
    'DISPID_SPRulesItem', 'IEnumSpObjectTokens',
    'DISPID_SRGSetTextSelection', 'ISpResourceManager',
    'DISPID_SOTGetAttribute', 'DISPID_SABufferNotifySize',
    'DISPID_SGRSAddRuleTransition',
    'DISPID_SpeechGrammarRuleStateTransition',
    'DISPID_SWFEBlockAlign', 'SP_VISEME_12', 'SAFT8kHz8BitStereo',
    'SPEI_MIN_SR', 'SDTAudio', 'SAFTCCITT_uLaw_22kHzMono',
    'SPWF_SRENGINE', 'DISPID_SWFEFormatTag', 'SPEI_WORD_BOUNDARY',
    'ISpeechRecognizerStatus', 'SpeechGrammarTagUnlimitedDictation',
    'DISPID_SMSSetData', 'ISpeechPhraseInfoBuilder', 'IStream',
    'SpeechAudioVolume', 'SPEI_RECO_STATE_CHANGE',
    'DISPID_SpeechObjectTokens', 'SPGS_EXCLUSIVE', 'DISPID_SVSkip',
    'SVEStartInputStream', 'DISPID_SOTSetId', 'DISPID_SPIEngineId',
    'DISPID_SPAPhraseInfo', 'DISPID_SRCRetainedAudioFormat',
    'SPVISEMES', 'DISPID_SRCEEnginePrivate',
    'DISPID_SpeechXMLRecoResult',
    'SPINTERFERENCE_LATENCY_TRUNCATE_END', 'ISpEventSource',
    'DISPID_SRCCreateResultFromMemory', 'WAVEFORMATEX',
    'ISpeechRecognizer', 'SAFTNonStandardFormat', 'SpShortcut',
    'SPLOADOPTIONS', 'DISPID_SLPPartOfSpeech', 'SRSActive',
    'SRTExtendableParse', 'SPSNotOverriden', 'ISpeechPhraseProperty',
    'SSSPTRelativeToEnd', 'ISpXMLRecoResult', 'STCInprocServer',
    'DISPID_SPILanguageId', 'SpeechRecognitionType',
    'DISPID_SLRemovePronunciationByPhoneIds',
    'SpeechPropertyResourceUsage', 'DISPID_SWFEBitsPerSample',
    'STCAll', 'SAFT44kHz8BitMono',
    'DISPID_SRGCmdLoadFromProprietaryGrammar',
    'SAFTCCITT_uLaw_8kHzMono', 'SDKLCurrentUser',
    'DISPID_SpeechRecoContextEvents', 'DISPID_SRDisplayUI',
    'SWPKnownWordPronounceable', 'DISPID_SRCVoicePurgeEvent',
    'DISPID_SGRSTPropertyName', 'DISPID_SRSSupportedLanguages',
    'SpeechPropertyResponseSpeed', 'ISpeechLexiconPronunciation',
    'SpObjectTokenCategory', 'DISPID_SRCRecognizer', 'SPINTERFERENCE',
    'DISPID_SRGetPropertyNumber', 'SECFDefault',
    'SpeechVoiceSpeakFlags', 'DISPID_SOTCategory',
    'DISPID_SRCEventInterests', 'SPSHT_OTHER', 'DISPID_SOTsItem',
    'DISPID_SRCESoundStart', 'DISPID_SpeechRecoResult',
    'eLEXTYPE_USER_SHORTCUT', 'SAFTGSM610_44kHzMono', 'SPAUDIOSTATUS',
    'DISPID_SASetState', 'DISPID_SMSALineId', 'DISPID_SDKEnumValues',
    'SpeechCategoryVoices', 'SPFM_OPEN_READWRITE',
    'Speech_StreamPos_Asap', 'DISPID_SOTGetDescription',
    'DISPID_SRGSetWordSequenceData', 'SPEI_START_SR_STREAM',
    'SPLO_STATIC', 'SREAdaptation', 'DISPID_SABIBufferSize',
    'DISPID_SpeechLexiconWords', 'DISPID_SBSWrite',
    'eLEXTYPE_RESERVED7', 'eLEXTYPE_PRIVATE11', 'DISPID_SPPName',
    'SpResourceManager', 'SVP_17', 'SPCS_DISABLED', 'SVSFIsFilename',
    'SRESoundEnd', 'DISPID_SpeechPhraseElement', 'SVSFParseMask',
    'DISPID_SFSOpen', 'SSSPTRelativeToStart', 'SLODynamic',
    'SPAS_CLOSED', 'SPSMF_UPS', 'SPEI_START_INPUT_STREAM',
    'DISPID_SpeechVoiceStatus', 'DISPID_SRSNumberOfActiveRules',
    'SPEVENTENUM', 'ISpeechPhraseElements',
    'DISPID_SpeechRecognizerStatus', 'DISPID_SADefaultFormat',
    'SPAR_High', 'DISPID_SRCEEndStream', 'SpeechTokenShellFolder',
    'ISpeechPhraseAlternate', 'SPSUnknown', 'SAFT44kHz8BitStereo',
    'DISPID_SRCEAdaptation', 'SECHighConfidence', 'eLEXTYPE_PRIVATE7',
    'SPBO_AHEAD', 'DISPID_SDKOpenKey', 'SP_VISEME_7',
    'DISPID_SLPsCount', 'SAFT32kHz8BitMono', 'SAFTGSM610_11kHzMono',
    'SpeechCategoryAudioIn', 'DISPID_SRRAudio', 'SPCT_COMMAND',
    'SPGRAMMARWORDTYPE', 'DISPID_SVSRunningState',
    'SPEI_INTERFERENCE', 'SVP_18', 'DISPID_SPAsItem',
    'SPPS_RESERVED4', 'SP_VISEME_3', 'DISPID_SOTs_NewEnum',
    'DISPID_SRCEFalseRecognition', 'DISPID_SDKEnumKeys',
    'SPRS_ACTIVE_WITH_AUTO_PAUSE', 'DISPID_SLWLangId',
    'SVSFParseAutodetect', 'DISPID_SpeechLexiconPronunciation',
    'SGDisplay', 'SP_VISEME_20', 'DISPID_SPEPronunciation',
    'DISPID_SRCEAudioLevel', 'SPADAPTATIONRELEVANCE',
    '_ISpeechRecoContextEvents', 'STSF_LocalAppData',
    'DISPID_SRCreateRecoContext', 'DISPID_SGRSTPropertyValue',
    'DISPID_SDKGetBinaryValue', 'SRSInactive',
    'DISPID_SVSInputWordLength', 'SVSFVoiceMask',
    'SAFTCCITT_uLaw_44kHzMono', 'eLEXTYPE_APP',
    'SpeechBookmarkOptions', 'SPPHRASERULE',
    'SpeechSpecialTransitionType',
    'DISPID_SpeechGrammarRuleStateTransitions', 'DISPID_SpeechVoice',
    'SpeechInterference', 'SAFT12kHz16BitMono', 'DISPID_SRCResume',
    'SAFTGSM610_22kHzMono', 'ISpStreamFormat', 'DISPID_SVPause',
    'DISPID_SRCEInterference', 'ISpeechAudioBufferInfo',
    'SREStreamEnd', 'SpUnCompressedLexicon', 'SPVPRIORITY',
    'Speech_Max_Pron_Length', 'DISPID_SpeechPhoneConverter',
    'SAFTADPCM_44kHzMono', 'SPWORD', 'SpLexicon', 'SVF_Emphasis',
    'DISPID_SVEEnginePrivate', 'ISpeechPhraseAlternates',
    'ISpeechRecoResult2', 'eLEXTYPE_PRIVATE14',
    'DISPID_SVEStreamStart', 'DISPID_SLWs_NewEnum', 'SLTApp',
    'DISPID_SAFGetWaveFormatEx', 'SAFTCCITT_uLaw_11kHzStereo',
    'DISPID_SLWsCount', 'DISPID_SRGCmdSetRuleState',
    'SPRST_ACTIVE_ALWAYS', 'DISPID_SPIAudioSizeBytes',
    'ISpRecognizer', 'SpeechAllElements',
    'DISPID_SVAudioOutputStream', 'DISPID_SLWsItem',
    'SPCT_SUB_COMMAND', 'DISPID_SPRules_NewEnum', 'ISpRecognizer2',
    'SPEI_ACTIVE_CATEGORY_CHANGED', 'DISPID_SPIGrammarId',
    'eWORDTYPE_DELETED', 'DISPID_SRGCmdLoadFromObject',
    'SpeechGrammarTagWildcard', 'DISPID_SPCIdToPhone',
    'DISPID_SGRsCommit', 'DISPID_SPIStartTime', 'ISpeechLexiconWords',
    'SPEI_RECO_OTHER_CONTEXT', 'SPPS_Function',
    'SAFTADPCM_44kHzStereo', 'SPEI_RESERVED3', 'SREInterference',
    'DISPID_SMSAMMHandle', 'SpeechVisemeFeature', 'SpeechLexiconType',
    'SAFT12kHz16BitStereo', 'SPBO_PAUSE', 'Library',
    'SAFTADPCM_11kHzMono', 'DISPID_SVAlertBoundary',
    'DISPID_SVSPhonemeId', 'SpPhoneticAlphabetConverter',
    'DISPID_SVGetAudioInputs', 'SDA_Consume_Leading_Spaces',
    'SAFTCCITT_ALaw_22kHzStereo', 'SPAS_PAUSE', 'eLEXTYPE_RESERVED4',
    'ISpeechRecoContext', 'eLEXTYPE_PRIVATE16', 'SAFT22kHz8BitMono',
    'SAFT22kHz8BitStereo', 'SAFT22kHz16BitStereo', 'SVP_0',
    'SAFT32kHz8BitStereo', 'SPXRO_SML', 'DISPID_SVDisplayUI',
    'SPFM_CREATE', 'DISPID_SRCSetAdaptationData', 'DISPID_SPRText',
    'SPFILEMODE', 'DISPID_SLPSymbolic', 'SGSExclusive',
    'SpeechPropertyComplexResponseSpeed', 'DISPID_SVEPhoneme',
    'ISpRecoCategory', 'DISPID_SPPBRestorePhraseFromMemory',
    'SRSEDone', 'DISPID_SpeechAudio', 'SPAS_STOP', 'SPPS_LMA',
    'SPINTERFERENCE_NOISE', 'SPCT_SLEEP', 'STCRemoteServer',
    'DISPID_SRSetPropertyNumber', 'SpMMAudioIn',
    'DISPID_SWFESamplesPerSec', 'SPSHT_EMAIL', 'SRAInterpreter',
    'DISPID_SGRSTsItem', 'DISPID_SPPId',
    'DISPID_SLAddPronunciationByPhoneIds',
    'DISPID_SPIGetDisplayAttributes', 'DISPIDSPTSI',
    'DISPID_SPRuleName', 'ISpRecoGrammar2', 'DISPID_SLWType',
    'ISpObjectWithToken', 'DISPID_SGRSAddSpecialTransition',
    'SAFTCCITT_uLaw_8kHzStereo', 'SRARoot',
    'ISpPhoneticAlphabetSelection', 'DISPID_SPIEnginePrivateData',
    'SRSActiveAlways', 'SP_VISEME_11', 'ISpRecoContext2',
    'SpeechWordType', 'DISPID_SpeechBaseStream', 'SPGRAMMARSTATE',
    'SPBO_NONE', 'DISPID_SRCEPhraseStart', 'SPEI_VOICE_CHANGE',
    'DISPID_SpeechFileStream', 'DISPID_SPRsItem', 'SWTAdded',
    'eLEXTYPE_LETTERTOSOUND', 'SPEI_SR_AUDIO_LEVEL',
    'SpeechPropertyHighConfidenceThreshold', 'SECFNoSpecialChars',
    'DISPID_SDKDeleteKey', 'ISpeechLexiconWord', 'SPDKL_CurrentUser',
    'ISpeechResourceLoader', 'SSFMCreate', 'DISPID_SOTsCount',
    'SPINTERFERENCE_NOSIGNAL', 'DISPID_SPPsItem', 'SVEAllEvents',
    'SPVOICESTATUS', 'SPINTERFERENCE_TOOLOUD', 'SPDKL_LocalMachine',
    'SAFT48kHz8BitMono', 'DISPID_SPIRetainedSizeBytes', 'SVEBookmark',
    'ISpMMSysAudio', 'SpeechCategoryRecoProfiles',
    'DISPID_SRCERequestUI', 'DISPID_SRRTimes',
    'ISpeechRecoResultDispatch', 'SPAO_NONE',
    'DISPIDSPTSI_SelectionLength', 'SPRECOGNIZERSTATUS',
    'SPEVENTSOURCEINFO', 'DISPID_SRCERecognitionForOtherContext',
    'SpeechTokenContext', 'SpeechDisplayAttributes', 'SINoSignal',
    'SAFT24kHz8BitMono', 'SPRST_INACTIVE_WITH_PURGE', 'SVP_16',
    'SDTLexicalForm', 'SSFMOpenReadWrite', 'DISPID_SpeechVoiceEvent',
    'SPWORDPRONUNCIATIONLIST', 'DISPID_SPRuleChildren',
    'DISPID_SOTCId', 'SpeechTokenKeyUI', 'SAFT24kHz16BitStereo',
    'DISPID_SPPEngineConfidence', 'SPSHT_Unknown', 'SVP_4',
    'SAFT44kHz16BitMono', 'SPEI_VISEME', 'SAFTCCITT_ALaw_11kHzMono',
    'DISPID_SRProfile', 'DISPID_SRCEPropertyNumberChange',
    'SPRST_INACTIVE', 'ISpeechTextSelectionInformation',
    'SREAudioLevel', 'SPGS_ENABLED', 'DISPID_SRRTStreamTime',
    'SpeechAudioProperties', 'DISPID_SRCVoice', 'SP_VISEME_10',
    'ISpPhoneticAlphabetConverter', 'SRADynamic', 'SGRSTTEpsilon',
    'DISPID_SGRAddState', 'SPEI_RESERVED6', 'DISPID_SLPs_NewEnum',
    'DISPID_SRRRecoContext', 'SAFT16kHz8BitStereo',
    'Speech_StreamPos_RealTime', 'SAFT24kHz8BitStereo',
    '__MIDL_IWinTypes_0009', 'DISPID_SPERetainedSizeBytes',
    'SRAImport', 'SpeechAddRemoveWord',
    'DISPID_SVSyncronousSpeakTimeout', 'SPEI_END_SR_STREAM',
    'SDTPronunciation', 'SpeechStreamSeekPositionType', 'SVP_15',
    'DISPID_SRRecognizer', 'DISPID_SPEAudioTimeOffset',
    'SPINTERFERENCE_NONE', 'DISPID_SOTGetStorageFileName',
    'DISPID_SGRsAdd', 'ISpEventSink', 'SpMMAudioEnum',
    'DISPID_SASCurrentDevicePosition', 'SpeechStreamFileMode',
    'DISPIDSPTSI_ActiveOffset', 'DISPID_SpeechAudioFormat',
    'SpeechGrammarWordType', 'SP_VISEME_16', 'DISPID_SPAsCount',
    'DISPID_SpeechAudioBufferInfo', 'SVSFIsNotXML',
    'DISPID_SpeechPhraseBuilder', 'eLEXTYPE_PRIVATE12',
    'ISpeechGrammarRules', 'SpMemoryStream',
    'ISpeechPhraseProperties', 'DISPID_SVEWord',
    'SpeechTokenKeyFiles', 'SpeechUserTraining',
    'DISPID_SPRuleConfidence', 'SVP_3', 'SpeechDataKeyLocation',
    'SRCS_Disabled', 'SVP_13', 'SRESoundStart', 'SpPhoneConverter',
    'typelib_path', 'SpeechVoiceCategoryTTSRate',
    'DISPID_SPIAudioStreamPosition', 'DISPID_SBSFormat',
    'SWPUnknownWordUnpronounceable', 'SPEI_MIN_TTS',
    'SPSERIALIZEDRESULT', 'SAFTCCITT_ALaw_44kHzMono', 'SP_VISEME_1',
    'SpeechEmulationCompareFlags', 'SPEI_HYPOTHESIS',
    'DISPID_SLWWord', 'SpeechCategoryRecognizers', 'SASRun',
    'ISpeechXMLRecoResult', 'DISPID_SRRGetXMLResult', 'ISpeechVoice',
    'DISPID_SRGReset', 'ISpeechPhraseReplacement',
    'DISPID_SPRs_NewEnum', 'DISPID_SLGetPronunciations',
    'SP_VISEME_6', 'eLEXTYPE_PRIVATE17', 'DISPID_SVEStreamEnd',
    'DISPID_SRRDiscardResultInfo', 'eLEXTYPE_PRIVATE18',
    'DISPID_SRCAudioInInterferenceStatus', 'Speech_Max_Word_Length',
    'SVF_Stressed', 'SWPUnknownWordPronounceable', 'SVEWordBoundary',
    'SPPS_Interjection', 'ISpStream', '_ISpeechVoiceEvents', 'SVP_8',
    'DISPID_SVSCurrentStreamNumber', 'SAFTCCITT_ALaw_44kHzStereo',
    'ISpeechRecoResultTimes', 'DISPID_SVResume', 'SPCONTEXTSTATE',
    'DISPID_SPEs_NewEnum', 'SGRSTTTextBuffer',
    'DISPID_SOTCEnumerateTokens', 'SpAudioFormat', 'SREBookmark',
    'DISPID_SRGCmdSetRuleIdState', 'SPSNoun', 'DISPID_SVGetVoices',
    'DISPID_SLWPronunciations', 'eLEXTYPE_RESERVED10',
    'SPEI_ADAPTATION', 'DISPID_SpeechGrammarRule', 'SPEI_RECOGNITION',
    'SFTSREngine', 'ISpRecognizer3', 'DISPID_SpeechDataKey',
    'SpeechAudioFormatGUIDText', 'SRCS_Enabled', 'SVP_2',
    'DISPID_SRRSaveToMemory', 'ISpeechPhraseRule',
    'DISPID_SpeechPhraseAlternates', 'eLEXTYPE_PRIVATE10',
    'SPLEXICONTYPE', 'SRTStandard', 'SVEAudioLevel',
    'SpeechEngineProperties', 'SINoise', 'DISPID_SRCEBookmark',
    'STSF_AppData', 'SAFT22kHz16BitMono', 'SpeechFormatType',
    'SVP_10', 'SPSModifier', 'DISPID_SPACommit', 'DISPID_SPIGetText',
    'DISPID_SPRuleEngineConfidence', 'SPEI_PHONEME', 'DISPID_SPIRule',
    'SP_VISEME_18', 'SpObjectToken', 'DISPID_SRGetPropertyString',
    'DISPID_SRCBookmark', 'ISpeechPhraseElement', 'SRAExport',
    'eLEXTYPE_RESERVED6', '_RemotableHandle', 'DISPID_SRRAlternates',
    'eWORDTYPE_ADDED', 'DISPID_SRRTOffsetFromStart',
    'DISPID_SpeechPhraseReplacements', 'SPWORDPRONOUNCEABLE',
    'DISPID_SPRuleFirstElement', 'SpMMAudioOut',
    'DISPID_SPANumberOfElementsInResult', 'DISPID_SDKSetBinaryValue',
    'DISPID_SVIsUISupported', 'STCLocalServer', 'SRTReSent',
    'SpeechVoiceEvents', 'SAFTGSM610_8kHzMono', 'DISPID_SGRClear',
    'DISPID_SPCPhoneToId', 'DISPID_SGRsCount', 'SRAORetainAudio',
    'SPSSuppressWord', 'DISPID_SRSetPropertyString', 'DISPID_SGRId',
    'DISPID_SPAStartElementInResult', 'DISPID_SpeechWaveFormatEx',
    'SPSERIALIZEDPHRASE', 'SPINTERFERENCE_LATENCY_TRUNCATE_BEGIN',
    'IInternetSecurityMgrSite', 'DISPID_SOTRemoveStorageFileName',
    'eLEXTYPE_PRIVATE9', 'SAFT12kHz8BitStereo', 'SITooSlow',
    'DISPID_SREmulateRecognition', 'DISPID_SAVolume',
    'DISPID_SRCRetainedAudio', 'ISpeechLexicon', 'SP_VISEME_19',
    'DISPID_SRGetFormat', 'ISpNotifySink', 'SPAR_Unknown',
    'SECFEmulateResult', 'SAFT8kHz16BitMono', 'SAFT48kHz16BitStereo',
    'DISPID_SGRAttributes', 'SPFM_NUM_MODES', 'SP_VISEME_8',
    'DISPID_SVSLastResult', 'SGPronounciation', 'SpeechRecoEvents',
    'SPPS_Noncontent', 'SPSHT_NotOverriden', 'eLEXTYPE_PRIVATE6',
    'SPWAVEFORMATTYPE', 'eLEXTYPE_PRIVATE19',
    'DISPID_SVGetAudioOutputs', 'UINT_PTR',
    'SpeechCategoryPhoneConverters', 'DISPID_SRCState',
    'DISPID_SRGIsPronounceable', 'STCInprocHandler',
    'DISPID_SpeechRecoResultTimes', 'DISPID_SLPsItem',
    'SpStreamFormatConverter', 'ISpSerializeState',
    'DISPID_SpeechPhraseRules', 'DISPID_SpeechPhraseInfo',
    'DISPID_SPEsItem', 'DISPID_SpeechPhraseProperties',
    'DISPID_SRAllowVoiceFormatMatchingOnNextSet', 'SGRSTTRule',
    'SPPHRASEELEMENT', 'SRTEmulated', 'SVEPhoneme',
    'ISpeechAudioStatus', 'DISPID_SRCEStartStream',
    'SPEI_SENTENCE_BOUNDARY', 'SpPhraseInfoBuilder',
    'DISPID_SPPChildren', 'SPSMF_SAPI_PROPERTIES',
    'DISPID_SGRSAddWordTransition', 'SAFTTrueSpeech_8kHz1BitMono',
    'SGSDisabled', 'DISPID_SPRuleParent', 'SECFIgnoreKanaType',
    'SpeechRetainedAudioOptions', 'SpWaveFormatEx',
    'SPCT_SUB_DICTATION', 'SRTSMLTimeout', 'DISPID_SBSRead',
    'DISPID_SRRSpeakAudio', 'eLEXTYPE_PRIVATE8', '_SPAUDIOSTATE',
    'ISpeechWaveFormatEx', 'ISpeechPhoneConverter',
    'DISPID_SLGetGenerationChange', 'SVSFNLPMask', 'ISpPhrase',
    'SPINTERFERENCE_TOOFAST', 'DISPID_SLRemovePronunciation',
    'SPSHORTCUTTYPE', 'SPPHRASE', 'SVPAlert',
    'ISpeechGrammarRuleStateTransition', 'SAFT24kHz16BitMono',
    'SPSLMA', 'DISPID_SVSLastBookmarkId', 'DISPID_SRGRecoContext',
    'DISPID_SpeechRecognizer', 'eLEXTYPE_PRIVATE5', 'SVEPrivate',
    'SDTDisplayText', 'DISPID_SOTId', 'SpeechDictationTopicSpelling',
    'eLEXTYPE_PRIVATE20', 'DISPID_SDKSetStringValue', 'SVP_11',
    'DISPID_SRState', 'DISPID_SRSCurrentStreamPosition',
    'IInternetSecurityManager', 'SPPS_Noun', 'SPEI_MAX_SR',
    'SpSharedRecognizer', 'SPEI_FALSE_RECOGNITION',
    'SpeechEngineConfidence', 'SPAR_Low', 'DISPID_SMSGetData',
    'SREStreamStart', 'SPFM_CREATE_ALWAYS', 'DISPID_SpeechPhraseRule',
    'eLEXTYPE_PRIVATE2', 'SPRECORESULTTIMES', 'SPEI_SOUND_END',
    'SpTextSelectionInformation', 'SPFM_OPEN_READONLY',
    'SpeechVoiceSkipTypeSentence', 'DISPID_SRGId',
    'DISPID_SRAudioInput', 'SpFileStream',
    'DISPID_SVSInputSentencePosition',
    'SPSMF_SRGS_SEMANTICINTERPRETATION_W3C', 'SpeechRecognizerState',
    'DISPID_SRRTLength', 'SAFTDefault',
    'DISPID_SPRuleNumberOfElements', 'DISPID_SRRAudioFormat',
    'DISPID_SPISaveToMemory', 'DISPID_SPPNumberOfElements',
    'DISPID_SVEVoiceChange', 'SpeechGrammarState', 'SPSVerb',
    'DISPID_SVRate', 'STSF_CommonAppData', 'SVP_19', 'SPCT_DICTATION',
    'SpeechWordPronounceable'
]

