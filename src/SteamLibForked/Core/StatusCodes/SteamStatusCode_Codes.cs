// ReSharper disable InconsistentNaming

namespace SteamLib.Core.StatusCodes;

public partial class SteamStatusCode //Codes 
{
    /// <summary>
    ///     Status code has no translation yet
    /// </summary>
    public static readonly SteamStatusCode Unknown = new(-1, nameof(Unknown));

    /// <summary>
    ///     Response ended without status code even though it should have been there
    /// </summary>
    public static readonly SteamStatusCode Undefined = new(0, nameof(Undefined));

    ///<summary>1</summary>
    public static readonly SteamStatusCode Ok = new(1, nameof(Ok));

    ///<summary>2</summary>
    public static readonly SteamStatusCode Fail = new(2, nameof(Fail));

    ///<summary>3</summary>
    public static readonly SteamStatusCode NoConnection = new(3, nameof(NoConnection));

    ///<summary>4</summary>
    public static readonly SteamStatusCode NoConnectionRetry = new(4, nameof(NoConnectionRetry));

    ///<summary>5</summary>
    public static readonly SteamStatusCode InvalidPassword = new(5, nameof(InvalidPassword));

    ///<summary>6</summary>
    public static readonly SteamStatusCode LoggedInElsewhere = new(6, nameof(LoggedInElsewhere));

    ///<summary>7</summary>
    public static readonly SteamStatusCode InvalidProtocolVer = new(7, nameof(InvalidProtocolVer));

    ///<summary>8</summary>
    public static readonly SteamStatusCode InvalidParam = new(8, nameof(InvalidParam));

    ///<summary>9</summary>
    public static readonly SteamStatusCode FileNotFound = new(9, nameof(FileNotFound));

    ///<summary>10</summary>
    public static readonly SteamStatusCode Busy = new(10, nameof(Busy));

    ///<summary>11</summary>
    public static readonly SteamStatusCode InvalidState = new(11, nameof(InvalidState));

    ///<summary>12</summary>
    public static readonly SteamStatusCode InvalidName = new(12, nameof(InvalidName));

    ///<summary>13</summary>
    public static readonly SteamStatusCode InvalidEmail = new(13, nameof(InvalidEmail));

    ///<summary>14</summary>
    public static readonly SteamStatusCode DuplicateName = new(14, nameof(DuplicateName));

    ///<summary>15</summary>
    public static readonly SteamStatusCode AccessDenied = new(15, nameof(AccessDenied));

    ///<summary>16</summary>
    public static readonly SteamStatusCode Timeout = new(16, nameof(Timeout));

    ///<summary>17</summary>
    public static readonly SteamStatusCode Banned = new(17, nameof(Banned));

    ///<summary>18</summary>
    public static readonly SteamStatusCode AccountNotFound = new(18, nameof(AccountNotFound));

    ///<summary>19</summary>
    public static readonly SteamStatusCode InvalidSteamID = new(19, nameof(InvalidSteamID));

    ///<summary>20</summary>
    public static readonly SteamStatusCode ServiceUnavailable = new(20, nameof(ServiceUnavailable));

    ///<summary>21</summary>
    public static readonly SteamStatusCode NotLoggedOn = new(21, nameof(NotLoggedOn));

    ///<summary>22</summary>
    public static readonly SteamStatusCode Pending = new(22, nameof(Pending));

    ///<summary>23</summary>
    public static readonly SteamStatusCode EncryptionFailure = new(23, nameof(EncryptionFailure));

    ///<summary>24</summary>
    public static readonly SteamStatusCode InsufficientPrivilege = new(24, nameof(InsufficientPrivilege));

    ///<summary>25</summary>
    public static readonly SteamStatusCode LimitExceeded = new(25, nameof(LimitExceeded));

    ///<summary>26</summary>
    public static readonly SteamStatusCode Revoked = new(26, nameof(Revoked));

    ///<summary>27</summary>
    public static readonly SteamStatusCode Expired = new(27, nameof(Expired));

    ///<summary>28</summary>
    public static readonly SteamStatusCode AlreadyRedeemed = new(28, nameof(AlreadyRedeemed));

    ///<summary>29</summary>
    public static readonly SteamStatusCode DuplicateRequest = new(29, nameof(DuplicateRequest));

    ///<summary>30</summary>
    public static readonly SteamStatusCode AlreadyOwned = new(30, nameof(AlreadyOwned));

    ///<summary>31</summary>
    public static readonly SteamStatusCode IPNotFound = new(31, nameof(IPNotFound));

    ///<summary>32</summary>
    public static readonly SteamStatusCode PersistFailed = new(32, nameof(PersistFailed));

    ///<summary>33</summary>
    public static readonly SteamStatusCode LockingFailed = new(33, nameof(LockingFailed));

    ///<summary>34</summary>
    public static readonly SteamStatusCode LogonSessionReplaced = new(34, nameof(LogonSessionReplaced));

    ///<summary>35</summary>
    public static readonly SteamStatusCode ConnectFailed = new(35, nameof(ConnectFailed));

    ///<summary>36</summary>
    public static readonly SteamStatusCode HandshakeFailed = new(36, nameof(HandshakeFailed));

    ///<summary>37</summary>
    public static readonly SteamStatusCode IOFailure = new(37, nameof(IOFailure));

    ///<summary>38</summary>
    public static readonly SteamStatusCode RemoteDisconnect = new(38, nameof(RemoteDisconnect));

    ///<summary>39</summary>
    public static readonly SteamStatusCode ShoppingCartNotFound = new(39, nameof(ShoppingCartNotFound));

    ///<summary>40</summary>
    public static readonly SteamStatusCode Blocked = new(40, nameof(Blocked));

    ///<summary>41</summary>
    public static readonly SteamStatusCode Ignored = new(41, nameof(Ignored));

    ///<summary>42</summary>
    public static readonly SteamStatusCode NoMatch = new(42, nameof(NoMatch));

    ///<summary>43</summary>
    public static readonly SteamStatusCode AccountDisabled = new(43, nameof(AccountDisabled));

    ///<summary>44</summary>
    public static readonly SteamStatusCode ServiceReadOnly = new(44, nameof(ServiceReadOnly));

    ///<summary>45</summary>
    public static readonly SteamStatusCode AccountNotFeatured = new(45, nameof(AccountNotFeatured));

    ///<summary>46</summary>
    public static readonly SteamStatusCode AdministratorOK = new(46, nameof(AdministratorOK));

    ///<summary>47</summary>
    public static readonly SteamStatusCode ContentVersion = new(47, nameof(ContentVersion));

    ///<summary>48</summary>
    public static readonly SteamStatusCode TryAnotherCM = new(48, nameof(TryAnotherCM));

    ///<summary>49</summary>
    public static readonly SteamStatusCode PasswordRequiredToKickSession =
        new(49, nameof(PasswordRequiredToKickSession));

    ///<summary>50</summary>
    public static readonly SteamStatusCode AlreadyLoggedInElsewhere = new(50, nameof(AlreadyLoggedInElsewhere));

    ///<summary>51</summary>
    public static readonly SteamStatusCode Suspended = new(51, nameof(Suspended));

    ///<summary>52</summary>
    public static readonly SteamStatusCode Cancelled = new(52, nameof(Cancelled));

    ///<summary>53</summary>
    public static readonly SteamStatusCode DataCorruption = new(53, nameof(DataCorruption));

    ///<summary>54</summary>
    public static readonly SteamStatusCode DiskFull = new(54, nameof(DiskFull));

    ///<summary>55</summary>
    public static readonly SteamStatusCode RemoteCallFailed = new(55, nameof(RemoteCallFailed));

    ///<summary>56</summary>
    public static readonly SteamStatusCode PasswordUnset = new(56, nameof(PasswordUnset));

    ///<summary>57</summary>
    public static readonly SteamStatusCode ExternalAccountUnlinked = new(57, nameof(ExternalAccountUnlinked));

    ///<summary>58</summary>
    public static readonly SteamStatusCode PSNTicketInvalid = new(58, nameof(PSNTicketInvalid));

    ///<summary>59</summary>
    public static readonly SteamStatusCode ExternalAccountAlreadyLinked = new(59, nameof(ExternalAccountAlreadyLinked));

    ///<summary>60</summary>
    public static readonly SteamStatusCode RemoteFileConflict = new(60, nameof(RemoteFileConflict));

    ///<summary>61</summary>
    public static readonly SteamStatusCode IllegalPassword = new(61, nameof(IllegalPassword));

    ///<summary>62</summary>
    public static readonly SteamStatusCode SameAsPreviousValue = new(62, nameof(SameAsPreviousValue));

    ///<summary>63</summary>
    public static readonly SteamStatusCode AccountLogonDenied = new(63, nameof(AccountLogonDenied));

    ///<summary>64</summary>
    public static readonly SteamStatusCode CannotUseOldPassword = new(64, nameof(CannotUseOldPassword));

    ///<summary>65</summary>
    public static readonly SteamStatusCode InvalidLoginAuthCode = new(65, nameof(InvalidLoginAuthCode));

    ///<summary>66</summary>
    public static readonly SteamStatusCode AccountLogonDeniedNoMail = new(66, nameof(AccountLogonDeniedNoMail));

    ///<summary>67</summary>
    public static readonly SteamStatusCode HardwareNotCapableOfIPT = new(67, nameof(HardwareNotCapableOfIPT));

    ///<summary>68</summary>
    public static readonly SteamStatusCode IPTInitError = new(68, nameof(IPTInitError));

    ///<summary>69</summary>
    public static readonly SteamStatusCode ParentalControlRestricted = new(69, nameof(ParentalControlRestricted));

    ///<summary>70</summary>
    public static readonly SteamStatusCode FacebookQueryError = new(70, nameof(FacebookQueryError));

    ///<summary>71</summary>
    public static readonly SteamStatusCode ExpiredLoginAuthCode = new(71, nameof(ExpiredLoginAuthCode));

    ///<summary>72</summary>
    public static readonly SteamStatusCode IPLoginRestrictionFailed = new(72, nameof(IPLoginRestrictionFailed));

    ///<summary>73</summary>
    public static readonly SteamStatusCode AccountLockedDown = new(73, nameof(AccountLockedDown));

    ///<summary>74</summary>
    public static readonly SteamStatusCode AccountLogonDeniedVerifiedEmailRequired =
        new(74, nameof(AccountLogonDeniedVerifiedEmailRequired));

    ///<summary>75</summary>
    public static readonly SteamStatusCode NoMatchingURL = new(75, nameof(NoMatchingURL));

    ///<summary>76</summary>
    public static readonly SteamStatusCode BadResponse = new(76, nameof(BadResponse));

    ///<summary>77</summary>
    public static readonly SteamStatusCode RequirePasswordReEntry = new(77, nameof(RequirePasswordReEntry));

    ///<summary>78</summary>
    public static readonly SteamStatusCode ValueOutOfRange = new(78, nameof(ValueOutOfRange));

    ///<summary>79</summary>
    public static readonly SteamStatusCode UnexpectedError = new(79, nameof(UnexpectedError));

    ///<summary>80</summary>
    public static readonly SteamStatusCode Disabled = new(80, nameof(Disabled));

    ///<summary>81</summary>
    public static readonly SteamStatusCode InvalidCEGSubmission = new(81, nameof(InvalidCEGSubmission));

    ///<summary>82</summary>
    public static readonly SteamStatusCode RestrictedDevice = new(82, nameof(RestrictedDevice));

    ///<summary>83</summary>
    public static readonly SteamStatusCode RegionLocked = new(83, nameof(RegionLocked));

    ///<summary>84</summary>
    public static readonly SteamStatusCode RateLimitExceeded = new(84, nameof(RateLimitExceeded));

    ///<summary>85</summary>
    public static readonly SteamStatusCode AccountLoginDeniedNeedTwoFactor =
        new(85, nameof(AccountLoginDeniedNeedTwoFactor));

    ///<summary>86</summary>
    public static readonly SteamStatusCode ItemDeleted = new(86, nameof(ItemDeleted));

    ///<summary>87</summary>
    public static readonly SteamStatusCode AccountLoginDeniedThrottle = new(87, nameof(AccountLoginDeniedThrottle));

    ///<summary>88</summary>
    public static readonly SteamStatusCode TwoFactorCodeMismatch = new(88, nameof(TwoFactorCodeMismatch));

    ///<summary>89</summary>
    public static readonly SteamStatusCode TwoFactorActivationCodeMismatch =
        new(89, nameof(TwoFactorActivationCodeMismatch));

    ///<summary>90</summary>
    public static readonly SteamStatusCode AccountAssociatedToMultiplePartners =
        new(90, nameof(AccountAssociatedToMultiplePartners));

    ///<summary>91</summary>
    public static readonly SteamStatusCode NotModified = new(91, nameof(NotModified));

    ///<summary>92</summary>
    public static readonly SteamStatusCode NoMobileDevice = new(92, nameof(NoMobileDevice));

    ///<summary>93</summary>
    public static readonly SteamStatusCode TimeNotSynced = new(93, nameof(TimeNotSynced));

    ///<summary>94</summary>
    public static readonly SteamStatusCode SmsCodeFailed = new(94, nameof(SmsCodeFailed));

    ///<summary>95</summary>
    public static readonly SteamStatusCode AccountLimitExceeded = new(95, nameof(AccountLimitExceeded));

    ///<summary>96</summary>
    public static readonly SteamStatusCode AccountActivityLimitExceeded = new(96, nameof(AccountActivityLimitExceeded));

    ///<summary>97</summary>
    public static readonly SteamStatusCode PhoneActivityLimitExceeded = new(97, nameof(PhoneActivityLimitExceeded));

    ///<summary>98</summary>
    public static readonly SteamStatusCode RefundToWallet = new(98, nameof(RefundToWallet));

    ///<summary>99</summary>
    public static readonly SteamStatusCode EmailSendFailure = new(99, nameof(EmailSendFailure));

    ///<summary>100</summary>
    public static readonly SteamStatusCode NotSettled = new(100, nameof(NotSettled));

    ///<summary>101</summary>
    public static readonly SteamStatusCode NeedCaptcha = new(101, nameof(NeedCaptcha));

    ///<summary>102</summary>
    public static readonly SteamStatusCode GSLTDenied = new(102, nameof(GSLTDenied));

    ///<summary>103</summary>
    public static readonly SteamStatusCode GSOwnerDenied = new(103, nameof(GSOwnerDenied));

    ///<summary>104</summary>
    public static readonly SteamStatusCode InvalidItemType = new(104, nameof(InvalidItemType));

    ///<summary>105</summary>
    public static readonly SteamStatusCode IPBanned = new(105, nameof(IPBanned));

    ///<summary>106</summary>
    public static readonly SteamStatusCode GSLTExpired = new(106, nameof(GSLTExpired));

    ///<summary>107</summary>
    public static readonly SteamStatusCode InsufficientFunds = new(107, nameof(InsufficientFunds));

    ///<summary>108</summary>
    public static readonly SteamStatusCode TooManyPending = new(108, nameof(TooManyPending));

    ///<summary>109</summary>
    public static readonly SteamStatusCode NoSiteLicensesFound = new(109, nameof(NoSiteLicensesFound));

    ///<summary>110</summary>
    public static readonly SteamStatusCode WGNetworkSendExceeded = new(110, nameof(WGNetworkSendExceeded));

    ///<summary>111</summary>
    public static readonly SteamStatusCode AccountNotFriends = new(111, nameof(AccountNotFriends));

    ///<summary>112</summary>
    public static readonly SteamStatusCode LimitedUserAccount = new(112, nameof(LimitedUserAccount));

    ///<summary>113</summary>
    public static readonly SteamStatusCode CantRemoveItem = new(113, nameof(CantRemoveItem));

    ///<summary>114</summary>
    public static readonly SteamStatusCode AccountDeleted = new(114, nameof(AccountDeleted));

    ///<summary>115</summary>
    public static readonly SteamStatusCode
        ExistingUserCancelledLicense = new(115, nameof(ExistingUserCancelledLicense));

    ///<summary>116</summary>
    public static readonly SteamStatusCode CommunityCooldown = new(116, nameof(CommunityCooldown));

    ///<summary>117</summary>
    public static readonly SteamStatusCode NoLauncherSpecified = new(117, nameof(NoLauncherSpecified));

    ///<summary>118</summary>
    public static readonly SteamStatusCode MustAgreeToSSA = new(118, nameof(MustAgreeToSSA));

    ///<summary>119</summary>
    public static readonly SteamStatusCode LauncherMigrated = new(119, nameof(LauncherMigrated));

    ///<summary>120</summary>
    public static readonly SteamStatusCode SteamRealmMismatch = new(120, nameof(SteamRealmMismatch));

    ///<summary>121</summary>
    public static readonly SteamStatusCode InvalidSignature = new(121, nameof(InvalidSignature));

    ///<summary>122</summary>
    public static readonly SteamStatusCode ParseFailure = new(122, nameof(ParseFailure));

    ///<summary>123</summary>
    public static readonly SteamStatusCode NoVerifiedPhone = new(123, nameof(NoVerifiedPhone));

    ///<summary>124</summary>
    public static readonly SteamStatusCode InsufficientBattery = new(124, nameof(InsufficientBattery));

    ///<summary>125</summary>
    public static readonly SteamStatusCode ChargerRequired = new(125, nameof(ChargerRequired));

    ///<summary>126</summary>
    public static readonly SteamStatusCode CachedCredentialInvalid = new(126, nameof(CachedCredentialInvalid));

    ///<summary>127</summary>
    public static readonly SteamStatusCode PhoneNumberIsVOIP = new(127, nameof(PhoneNumberIsVOIP));
}