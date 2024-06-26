// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Console;

using static Ansi;

internal class PrettyConsoleFormatterColorTheme : IPrettyConsoleFormatterTheme
{
    //                                             T=Timestamp
    //                                             │   I=TraceId
    //                                             │   │   L=Level
    //                                             │   │   │   M=Message
    //                                             │   │   │   │   R=Reset+Message
    //                                             │   │   │   │   │
    public Styles VerboseStyles     { get; } = new(TV, IV, LV, MV, RV); // V = Verbose
    public Styles InformationStyles { get; } = new(TI, II, LI, MI, RI); // I = Information
    public Styles WarningStyles     { get; } = new(TW, IW, LW, MW, RW); // W = Warning
    public Styles ErrorStyles       { get; } = new(TE, IE, LE, ME, RE); // E = Error
    public Styles CriticalStyles    { get; } = new(TC, IC, LC, MC, RC); // C = Critical

    public Styles GetStyles(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Information => InformationStyles,
            LogLevel.Warning     => WarningStyles,
            LogLevel.Error       => ErrorStyles,
            LogLevel.Critical    => CriticalStyles,
            _                    => VerboseStyles,
        };
    }

    private const string?
        // Timestamp
        TV = Begin + Reset + And + ForeBrightBlack + And + Fore256 + "239" + End,
        TI = Begin + Reset + And + ForeWhite       + And + Fore256 + "242" + End,
        TW = TI, // same as above
        TE = TI, // same as above
        TC = TI, // same as above

        // TraceId
        IV = Begin + ForeBlue + And + Fore256 + "23" + End,
        II = Begin + ForeCyan + And + Fore256 + "31" + End,
        IW = II, // same as above
        IE = II, // same as above
        IC = II, // same as above

        // Level
        LV = Begin + ForeBrightBlack + And + Fore256 + "243"          + End,
        LI = Begin + ForeDefault                                      + End,
        LW = Begin + ForeYellow                                       + End,
        LE = Begin + ForeBrightWhite + And + BackRed     + And + Bold + End,
        LC = Begin + ForeBrightWhite + And + BackMagenta + And + Bold + End,

        // Message
        MV = null, // keep level style
        MI = null, // keep level style
        MW = null, // keep level style
        ME = Begin + ForeBrightRed     + And + BackDefault + End,
        MC = Begin + ForeBrightMagenta + And + BackDefault + End,

        // Reset + Message
        RV = Begin + Reset + And + ForeBrightBlack + And + Fore256 + "243" + End,
        RI = Begin + Reset                                                 + End,
        RW = Begin + Reset + And + ForeYellow                              + End,
        RE = Begin + Reset + And + ForeBrightRed     + And + Bold          + End,
        RC = Begin + Reset + And + ForeBrightMagenta + And + Bold          + End;
}
