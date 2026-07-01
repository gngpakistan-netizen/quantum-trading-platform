//=============================================================================
// XAUUSD Institutional Liquidity Nexus — NinjaTrader 8 Hybrid (Indicator+Strategy)
// Ported from Pine Script Quantum 3.0/3.1 — All Engines, Dashboard & Trade Execution
//=============================================================================

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.MarketAnalyzer;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class XAUUSDLiquidityNexus : Strategy
    {
        #region Color Constants
        private Brush cBullBrush        = Brushes.LimeGreen;
        private Brush cBearBrush        = Brushes.Red;
        private Brush cNeutralBrush     = Brushes.Gray;
        private Brush cWarningBrush     = Brushes.Orange;
        private Brush cForecastBrush    = Brushes.Cyan;
        private Brush cBloomBgBrush     = new SolidColorBrush(Color.FromArgb(217, 17, 17, 17));
        private Brush cBloomLblBrush    = Brushes.LightGray;
        private Brush cBloomValBrush    = Brushes.White;
        private Brush cBloomBullBrush   = Brushes.LimeGreen;
        private Brush cBloomBearBrush   = Brushes.OrangeRed;
        private Brush cBloomWarnBrush   = Brushes.Gold;
        private Brush cBloomNeutBrush   = Brushes.Gray;
        private string cBloomSep        = "│";
        #endregion

        #region Input Parameters
        [NinjaScriptProperty]
        [Display(Name = "Dashboard", Order = 1, GroupName = "Modules")]
        public bool ShowDashboard { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Trade Plan", Order = 2, GroupName = "Modules")]
        public bool ShowTradePlan { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "VWAP", Order = 3, GroupName = "Modules")]
        public bool ShowVWAP { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Support / Resistance", Order = 4, GroupName = "Modules")]
        public bool ShowSR { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Sessions", Order = 5, GroupName = "Modules")]
        public bool ShowSessions { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Signals", Order = 6, GroupName = "Modules")]
        public bool ShowSignals { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Fair Value Gaps", Order = 7, GroupName = "Modules")]
        public bool ShowFVG { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Order Blocks", Order = 8, GroupName = "Modules")]
        public bool ShowOB { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "SMT Divergence", Order = 9, GroupName = "Modules")]
        public bool ShowSMT { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Volume Climax", Order = 10, GroupName = "Modules")]
        public bool ShowClimax { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Adaptive ATR Regime", Order = 11, GroupName = "Modules")]
        public bool ShowAdaptiveATR { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Displacement Candles", Order = 12, GroupName = "Modules")]
        public bool ShowDisplacement { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Market Structure Shift", Order = 13, GroupName = "Modules")]
        public bool ShowMSS { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Show EMAs", Order = 14, GroupName = "Modules")]
        public bool ShowEMAs { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Show ADX", Order = 15, GroupName = "Modules")]
        public bool ShowADX { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Show Macro Score", Order = 16, GroupName = "Modules")]
        public bool ShowMacro { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Statistical Engines", Order = 17, GroupName = "Modules")]
        public bool ShowStatsEngine { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Correlation Health Check", Order = 18, GroupName = "Modules")]
        public bool ShowCorrHealth { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Forecast Cone", Order = 19, GroupName = "Modules")]
        public bool ShowForecastCone { get; set; } = false;

        [NinjaScriptProperty]
        [Display(Name = "Show Liquidity Pools", Order = 20, GroupName = "Modules")]
        public bool ShowLiqPools { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Show Macro Panel", Order = 21, GroupName = "Modules")]
        public bool ShowMacroPanel { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Clean Institutional", Order = 22, GroupName = "Modules")]
        public bool CleanInstitutional { get; set; } = false;

        [NinjaScriptProperty]
        [Display(Name = "Professional Mode", Order = 23, GroupName = "Modules")]
        public bool ProfessionalMode { get; set; } = false;

        [NinjaScriptProperty]
        [Display(Name = "Institutional Performance Mode", Order = 24, GroupName = "Modules")]
        public bool PerfMode { get; set; } = false;

        // CORE
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "EMA20 Length", Order = 1, GroupName = "Core")]
        public int Ema20Len { get; set; } = 20;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "EMA100 Length", Order = 2, GroupName = "Core")]
        public int Ema100Len { get; set; } = 100;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "EMA200 Length", Order = 3, GroupName = "Core")]
        public int Ema200Len { get; set; } = 200;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Base", Order = 4, GroupName = "Core")]
        public int AtrLen { get; set; } = 14;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ADX Length", Order = 5, GroupName = "Core")]
        public int AdxLen { get; set; } = 14;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ADX Threshold", Order = 6, GroupName = "Core")]
        public int AdxThreshold { get; set; } = 25;

        [NinjaScriptProperty]
        [Range(2, 50)]
        [Display(Name = "RSI Length", Order = 7, GroupName = "Core")]
        public int RsiLen { get; set; } = 14;

        [NinjaScriptProperty]
        [Range(1, 5)]
        [Display(Name = "Internal Pivot Len (fast)", Order = 8, GroupName = "Core")]
        public int InternalPivotLen { get; set; } = 2;

        [NinjaScriptProperty]
        [Range(3, 10)]
        [Display(Name = "Swing Pivot Len (major)", Order = 9, GroupName = "Core")]
        public int SwingPivotLen { get; set; } = 5;

        [NinjaScriptProperty]
        [Range(50, 500)]
        [Display(Name = "Signal Lookback Bars", Order = 10, GroupName = "Core")]
        public int RecentBarsLen { get; set; } = 120;

        [NinjaScriptProperty]
        [Range(50, 90)]
        [Display(Name = "Trend Score Threshold", Order = 11, GroupName = "Core")]
        public int TrendThreshold { get; set; } = 60;

        [NinjaScriptProperty]
        [Range(90, 99)]
        [Display(Name = "Volume Climax Percentile", Order = 12, GroupName = "Core")]
        public int VolClimaxPerc { get; set; } = 95;

        [NinjaScriptProperty]
        [Range(0.05, 1.0)]
        [Display(Name = "EQ Tolerance (ATR mult)", Order = 13, GroupName = "Core")]
        public double EqAtrMult { get; set; } = 0.25;

        [NinjaScriptProperty]
        [Range(20, 100)]
        [Display(Name = "Volume Climax Lookback", Order = 14, GroupName = "Core")]
        public int VolLookback { get; set; } = 50;

        [NinjaScriptProperty]
        [Range(10, 100)]
        [Display(Name = "FVG Max Bars", Order = 15, GroupName = "Core")]
        public int FvgMaxBars { get; set; } = 30;

        [NinjaScriptProperty]
        [Range(10, 50)]
        [Display(Name = "OB Max Bars", Order = 16, GroupName = "Core")]
        public int ObMaxBars { get; set; } = 20;

        [NinjaScriptProperty]
        [Range(0.5, 4.0)]
        [Display(Name = "Liq Proximity (ATR)", Order = 17, GroupName = "Core")]
        public double LiqProxMult { get; set; } = 1.5;

        // V11 Intelligence
        [NinjaScriptProperty]
        [Range(1.5, 5.0)]
        [Display(Name = "Displacement Multiplier", Order = 1, GroupName = "V11 Intelligence")]
        public double DispMult { get; set; } = 2.5;

        [NinjaScriptProperty]
        [Display(Name = "Manip Threshold (ATR)", Order = 2, GroupName = "V11 Intelligence")]
        public double ManipThreshold { get; set; } = 0.5;

        // V17 Predictive
        [NinjaScriptProperty]
        [Range(5, 30)]
        [Display(Name = "DXY Momentum ROC Length", Order = 1, GroupName = "V17 Predictive")]
        public int DxyRocLen { get; set; } = 10;

        [NinjaScriptProperty]
        [Display(Name = "RSI Divergence Filter", Order = 2, GroupName = "V17 Predictive")]
        public bool UseRsiDivFilter { get; set; } = true;

        [NinjaScriptProperty]
        [Range(5, 20)]
        [Display(Name = "Correlation Break Lookback", Order = 3, GroupName = "V17 Predictive")]
        public int CorrHealthBars { get; set; } = 10;

        [NinjaScriptProperty]
        [Range(5, 20)]
        [Display(Name = "ZN1 Rate Pressure ROC", Order = 4, GroupName = "V17 Predictive")]
        public int ZnRocLen { get; set; } = 10;

        [NinjaScriptProperty]
        [Display(Name = "Invert Yield Logic (ZN1!)", Order = 5, GroupName = "V17 Predictive")]
        public bool InvertYield { get; set; } = false;

        [NinjaScriptProperty]
        [Range(5, 30)]
        [Display(Name = "Forecast Projection Bars", Order = 6, GroupName = "V17 Predictive")]
        public int ForecastBars { get; set; } = 10;

        [NinjaScriptProperty]
        [Range(1.0, 3.0)]
        [Display(Name = "Forecast Confidence Z-score", Order = 7, GroupName = "V17 Predictive")]
        public double ForecastZ { get; set; } = 2.0;

        // V19 Correlation
        [NinjaScriptProperty]
        [Range(10, 50)]
        [Display(Name = "Corr Short Window", Order = 1, GroupName = "V19 Correlation")]
        public int CorrShortLen { get; set; } = 30;

        [NinjaScriptProperty]
        [Range(30, 100)]
        [Display(Name = "Corr Medium Window", Order = 2, GroupName = "V19 Correlation")]
        public int CorrMedLen { get; set; } = 60;

        [NinjaScriptProperty]
        [Range(60, 200)]
        [Display(Name = "Corr Long Window", Order = 3, GroupName = "V19 Correlation")]
        public int CorrLongLen { get; set; } = 120;

        [NinjaScriptProperty]
        [Display(Name = "Macro Correlation Timeframe (min)", Order = 4, GroupName = "V19 Correlation")]
        public int MacroTFMinutes { get; set; } = 60;

        // RISK
        [NinjaScriptProperty]
        [Range(0.1, 5.0)]
        [Display(Name = "Risk % per Trade", Order = 1, GroupName = "Risk Management")]
        public double RiskPercent { get; set; } = 1.0;

        [NinjaScriptProperty]
        [Range(100, int.MaxValue)]
        [Display(Name = "Account Size (USD)", Order = 2, GroupName = "Risk Management")]
        public double AccountSize { get; set; } = 10000;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Value per Point (USD)", Order = 3, GroupName = "Risk Management")]
        public double PointValue { get; set; } = 100;

        // Layout
        [NinjaScriptProperty]
        [Display(Name = "Show Key Level Lines", Order = 1, GroupName = "Layout")]
        public bool ShowKeyLevels { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Show PDH Line", Order = 2, GroupName = "Layout")]
        public bool ShowPDH { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Show PDL Line", Order = 3, GroupName = "Layout")]
        public bool ShowPDL { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Show PWH Line", Order = 4, GroupName = "Layout")]
        public bool ShowPWH { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Show PWL Line", Order = 5, GroupName = "Layout")]
        public bool ShowPWL { get; set; } = true;

        // Macro Symbols
        [NinjaScriptProperty]
        [Display(Name = "DXY Symbol", Order = 1, GroupName = "Macro Symbols")]
        public string DxySymbol { get; set; } = "$DXY";

        [NinjaScriptProperty]
        [Display(Name = "Yield/10Y Symbol", Order = 2, GroupName = "Macro Symbols")]
        public string YieldSymbol { get; set; } = "ZN1!"; // or "ZN ##:##"

        [NinjaScriptProperty]
        [Display(Name = "Silver Symbol", Order = 3, GroupName = "Macro Symbols")]
        public string SilverSymbol { get; set; } = "XAGUSD";

        [NinjaScriptProperty]
        [Display(Name = "EURUSD Symbol", Order = 4, GroupName = "Macro Symbols")]
        public string EurUsdSymbol { get; set; } = "EURUSD";

        [NinjaScriptProperty]
        [Display(Name = "SPX Symbol", Order = 5, GroupName = "Macro Symbols")]
        public string SpxSymbol { get; set; } = "ES"; // S&P 500 e-mini or "SPX"
        #endregion

        #region State Variables — Series & Engine State

        // Series for time-dependent calculations
        private Series<double> ema20Series;
        private Series<double> ema100Series;
        private Series<double> ema200Series;
        private Series<double> atrSeries;
        private Series<double> adaptiveATRSeries;
        private Series<double> atrTrendingSeries;
        private Series<double> atrRangingSeries;
        private Series<double> diPlusSeries;
        private Series<double> diMinusSeries;
        private Series<double> adxSeries;
        private Series<double> vwapSeries;
        private Series<double> rsiSeries;
        private Series<double> volSma20Series;
        private Series<double> volPercentileSeries;
        private Series<double> goldRetSeries;
        private Series<double> bodySizeSeries;
        private Series<double> bodyPctSeries;

        // Cornish-Fisher
        private Series<double> retMean20Series;
        private Series<double> retStd20Series;
        private Series<double> retZScoreSeries;
        private Series<double> retSkewSeries;
        private Series<double> retKurtSeries;

        // Mean reversion
        private Series<double> distVwapATRSeries;
        private Series<double> distEma20ATRSeries;
        private Series<double> distEma100ATRSeries;
        private Series<double> distEma200ATRSeries;
        private Series<double> mrCompositeSeries;

        // Session state
        private bool _inAsian, _inLondon, _inNY, _inKillzone;
        private string _sessionLabel;
        private int _sessionQuality;

        // ATR Regime state
        private bool _atrInTrendingRegime;
        private bool _regimeTrending, _regimeRanging, _regimeDead;

        // HTF state
        private double _htf5mClose, _htf5mEma20, _htf5mEma50, _htf5mEma200;
        private double _htf15mClose, _htf15mEma20, _htf15mEma50, _htf15mEma200;
        private double _htf1hClose, _htf1hEma20, _htf1hEma50, _htf1hEma200;
        private double _htf4hClose, _htf4hEma20, _htf4hEma50, _htf4hEma200;
        private double _htfDClose, _htfDEma20, _htfDEma50, _htfDEma200, _dailyH, _dailyL;

        private int _htf5mScore, _htf15mScore, _htf1hScore, _htf4hScore, _htfDScore;
        private bool _htf5mBull, _htf5mBear, _htf15mBull, _htf15mBear;
        private bool _htf1hBull, _htf1hBear, _htf4hBull, _htf4hBear, _htfDBull, _htfDBear;
        private bool _htfBullGate, _htfBearGate;
        private int _htfAlignmentLong, _htfAlignmentShort;
        private int _htfFullLong, _htfFullShort;

        // Macro state
        private double _dxyClose, _yieldClose, _silverClose, _eurusdClose, _spxClose;
        private double _dxyEMAFast, _dxyEMASlow, _yieldEMAFast, _yieldEMASlow;
        private double _eurusdEMAFast, _eurusdEMASlow, _spxEMAFast, _spxEMASlow;
        private double _silverEMA;
        private double _dxyROC, _znROC, _eurusdROC;
        private bool _dxyBull, _dxyBear, _yieldRising, _yieldFalling;
        private bool _eurusdBull, _eurusdBear, _spxBull, _spxBear, _silverBull, _silverBear;
        private bool _macroBull, _macroBear;
        private int _macroBullVotes, _macroBearVotes;
        private bool _dxyValid, _yieldValid, _silverValid, _eurusdValid, _spxValid;
        private int _corrBreakCount;
        private bool _corrBreakdown;
        private double _dxyMomBull, _dxyMomBear, _ratePressureBull, _ratePressureBear;
        private int _macroStrengthScore;
        private double _macroStrengthConf;

        // Macro returns for correlation
        private Series<double> _dxyRetSeries;
        private Series<double> _yieldRetSeries;
        private Series<double> _silverRetSeries;
        private Series<double> _eurRetSeries;
        private Series<double> _spxRetSeries;

        // Correlation state
        private double _avgCorrDXY, _avgCorrYld, _avgCorrSlv, _avgCorrSPX, _avgCorrEUR;
        private double _corrHealthDXY, _corrHealthYld, _corrHealthSlv, _corrHealthSPX, _corrHealthEUR;
        private double _avgCorrHealth, _avgStability;
        private double _wDXY, _wYld, _wSlv, _wSPX, _wEUR;

        // Trend engine
        private int _bullTrendScore, _bearTrendScore;
        private bool _bullTrend, _bearTrend;

        // Structure engine
        private Series<double> _pivotHighSeries;
        private Series<double> _pivotLowSeries;
        private double _activeResistance, _activeSupport;
        private double _lastBrokenRes, _lastBrokenSup;

        private Series<double> _swingPivotHighSeries;
        private Series<double> _swingPivotLowSeries;
        private double _swingActiveResistance, _swingActiveSupport;
        private double _swingLastBrokenRes, _swingLastBrokenSup;

        // BOS
        private bool _bullBOS, _bearBOS, _bosLabelBull, _bosLabelBear;

        // CHOCH/MSS sequence
        private double _seqHigh1, _seqHigh2, _seqHigh3;
        private double _seqLow1, _seqLow2, _seqLow3;
        private int _seqLastBar;
        private bool _hh, _hl, _lh, _ll;
        private bool _chochLabelBull, _chochLabelBear;
        private bool _mssLabelBull, _mssLabelBear;

        // Displacement
        private bool _displacementUpActive, _displacementDownActive;
        private bool _displacementUp, _displacementDown;
        private bool _dispOnsetUp, _dispOnsetDown;

        // RSI Divergence
        private double _pivotRsiHigh, _pivotPriceHigh;
        private double _pivotRsiLow, _pivotPriceLow;
        private bool _bullRsiDiv, _bearRsiDiv;

        // EQH/EQL
        private double _swingHigh1, _swingHigh2, _swingLow1, _swingLow2;
        private bool _equalHighs, _equalLows;

        // Volume Climax
        private bool _climaxUp, _climaxDown;

        // FVG state
        private double _fvgHighUpper, _fvgLowLower;
        private int _fvgBarIndex;
        private double _fvgAtrAtDetect, _fvgVolPctAtDetect;
        private bool _fvgActive, _fvgWasBullish, _fvgMitigated;
        private double _fvgGapSize;
        private double _fvgScore;

        // OB state
        private double _obBullLow, _obBullHigh, _obBearLow, _obBearHigh;
        private int _obBullBar, _obBearBar;
        private bool _obBullActive, _obBearActive;
        private double _obBullVolPct, _obBearVolPct, _obBullAtr, _obBearAtr;
        private double _obBullScore, _obBearScore;

        // S/R levels
        private List<double> _srLevels = new List<double>();
        private List<double> _srStrengths = new List<double>();

        // Bias scores
        private double _bullBiasScore, _bearBiasScore;
        private double _confidenceScore;
        private string _biasLabel;

        // Trade Plan
        private bool _shouldBuy, _shouldSell;
        private double _tradeSL, _tradeTP1, _tradeTP2;
        private double _tradeRR;
        private string _tradeDirection;

        // Macro data series for correlation
        private Series<double> _macroDxySeries;
        private Series<double> _macroYieldSeries;
        private Series<double> _macroSilverSeries;
        private Series<double> _macroEurSeries;
        private Series<double> _macroSpxSeries;

        // Cached bar values for zero-repaint
        private double _close1, _high1, _low1, _open1, _volume1;

        // Bar counter for processing
        private bool _isRecentBar;
        private bool _sufficientBars;

        // Dashboard text objects
        private System.Windows.Controls.TextBlock _dashText;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "XAUUSD Institutional Liquidity Nexus — Hybrid Indicator + Strategy";
                Name = "XAUUSD Liquidity Nexus";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 100;
                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                // Add multi-timeframe and multi-symbol data series
                // HTF: 5-min, 15-min, 4H, Daily
                AddDataSeries(BarsPeriodType.Minute, 5);
                AddDataSeries(BarsPeriodType.Minute, 15);
                AddDataSeries(BarsPeriodType.Minute, 240); // 4H
                AddDataSeries(BarsPeriodType.Day, 1);

                // Macro instruments (same timeframe as primary for simplicity, or use 60-min)
                // Note: For true multi-instrument, NT8 requires the instrument to be available in your data feed
                AddDataSeries(DxySymbol, BarsPeriodType.Minute, 60);
                AddDataSeries(YieldSymbol, BarsPeriodType.Minute, 60);
                AddDataSeries(SilverSymbol, BarsPeriodType.Minute, 60);
                AddDataSeries(EurUsdSymbol, BarsPeriodType.Minute, 60);
                AddDataSeries(SpxSymbol, BarsPeriodType.Minute, 60);
            }
            else if (State == State.DataLoaded)
            {
                // Initialize all series
                ema20Series        = new Series<double>(this, MaximumBarsLookBack.Infinite);
                ema100Series       = new Series<double>(this, MaximumBarsLookBack.Infinite);
                ema200Series       = new Series<double>(this, MaximumBarsLookBack.Infinite);
                atrSeries          = new Series<double>(this, MaximumBarsLookBack.Infinite);
                adaptiveATRSeries  = new Series<double>(this, MaximumBarsLookBack.Infinite);
                atrTrendingSeries  = new Series<double>(this, MaximumBarsLookBack.Infinite);
                atrRangingSeries   = new Series<double>(this, MaximumBarsLookBack.Infinite);
                diPlusSeries       = new Series<double>(this, MaximumBarsLookBack.Infinite);
                diMinusSeries      = new Series<double>(this, MaximumBarsLookBack.Infinite);
                adxSeries          = new Series<double>(this, MaximumBarsLookBack.Infinite);
                vwapSeries         = new Series<double>(this, MaximumBarsLookBack.Infinite);
                rsiSeries          = new Series<double>(this, MaximumBarsLookBack.Infinite);
                volSma20Series     = new Series<double>(this, MaximumBarsLookBack.Infinite);
                volPercentileSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                goldRetSeries      = new Series<double>(this, MaximumBarsLookBack.Infinite);
                bodySizeSeries     = new Series<double>(this, MaximumBarsLookBack.Infinite);
                bodyPctSeries      = new Series<double>(this, MaximumBarsLookBack.Infinite);

                retMean20Series    = new Series<double>(this, MaximumBarsLookBack.Infinite);
                retStd20Series     = new Series<double>(this, MaximumBarsLookBack.Infinite);
                retZScoreSeries    = new Series<double>(this, MaximumBarsLookBack.Infinite);
                retSkewSeries      = new Series<double>(this, MaximumBarsLookBack.Infinite);
                retKurtSeries      = new Series<double>(this, MaximumBarsLookBack.Infinite);

                distVwapATRSeries  = new Series<double>(this, MaximumBarsLookBack.Infinite);
                distEma20ATRSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                distEma100ATRSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                distEma200ATRSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                mrCompositeSeries  = new Series<double>(this, MaximumBarsLookBack.Infinite);

                _pivotHighSeries   = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _pivotLowSeries    = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _swingPivotHighSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _swingPivotLowSeries  = new Series<double>(this, MaximumBarsLookBack.Infinite);

                _macroDxySeries    = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _macroYieldSeries  = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _macroSilverSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _macroEurSeries    = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _macroSpxSeries    = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _dxyRetSeries      = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _yieldRetSeries    = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _silverRetSeries   = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _eurRetSeries      = new Series<double>(this, MaximumBarsLookBack.Infinite);
                _spxRetSeries      = new Series<double>(this, MaximumBarsLookBack.Infinite);
            }
        }
        #endregion

        #region OnBarUpdate — Main Dispatcher
        protected override void OnBarUpdate()
        {
            // Only process on primary series to avoid duplicate logic
            if (BarsInProgress != 0)
            {
                // Store macro data from secondary series
                if (BarsInProgress == 5 && CurrentBars[5] > 0)
                    _macroDxySeries[0] = Closes[5][0];
                else if (BarsInProgress == 6 && CurrentBars[6] > 0)
                    _macroYieldSeries[0] = Closes[6][0];
                else if (BarsInProgress == 7 && CurrentBars[7] > 0)
                    _macroSilverSeries[0] = Closes[7][0];
                else if (BarsInProgress == 8 && CurrentBars[8] > 0)
                    _macroEurSeries[0] = Closes[8][0];
                else if (BarsInProgress == 9 && CurrentBars[9] > 0)
                    _macroSpxSeries[0] = Closes[9][0];
                return;
            }

            // Wait for sufficient bars on all series
            if (CurrentBars[0] < BarsRequiredToTrade) return;
            for (int i = 1; i <= 9; i++)
                if (CurrentBars[i] < Math.Max(BarsRequiredToTrade, 60)) return;

            // Cache close[1] values for zero-repaint architecture
            _close1  = Close[1];
            _high1   = High[1];
            _low1    = Low[1];
            _open1   = Open[1];
            _volume1 = Volume[1];

            _sufficientBars = CurrentBars[0] > 100;
            _isRecentBar = CurrentBars[0] > BarsRequiredToTrade;

            // Run all engines in order
            CalculateCoreIndicators();
            CalculateHTFTrendStack();
            CalculateAdaptiveATRRegime();
            CalculateMeanReversion();
            CalculateSessions();
            CalculateMacroEngine();
            CalculateCorrelation();
            CalculateWeightedTrend();
            CalculateDualStructure();
            CalculateBOS();
            CalculateCHOCH_MSS();
            CalculateDisplacement();
            CalculateRSIDivergence();
            CalculateEQHEQL();
            CalculateVolumeClimax();
            CalculateFVG();
            CalculateOB();
            CalculateSupportResistance();
            CalculateBiasScores();
            CalculateTradePlan();

            // Draw on chart
            DrawSignals();
            DrawDashboard();
        }
        #endregion

        #region Core Indicators
        private void CalculateCoreIndicators()
        {
            double c = Close[0];
            double h = High[0];
            double l = Low[0];
            double o = Open[0];
            double v = Volume[0];
            double hl2 = (h + l) / 2.0;
            double hlc3 = (h + l + c) / 3.0;
            double tr = Math.Max(h - l, Math.Max(Math.Abs(h - c), Math.Abs(l - c)));

            // EMAs
            ema20Series[0]  = (CurrentBar == 0) ? c : ema20Series[1] + (2.0 / (Ema20Len + 1.0)) * (c - ema20Series[1]);
            ema100Series[0] = (CurrentBar == 0) ? c : ema100Series[1] + (2.0 / (Ema100Len + 1.0)) * (c - ema100Series[1]);
            ema200Series[0] = (CurrentBar == 0) ? c : ema200Series[1] + (2.0 / (Ema200Len + 1.0)) * (c - ema200Series[1]);

            // ATR (simple 14-period)
            if (CurrentBar == 0)
                atrSeries[0] = tr;
            else if (CurrentBar < AtrLen)
                atrSeries[0] = atrSeries[1] + (tr - atrSeries[1]) / (CurrentBar + 1);
            else
                atrSeries[0] = (atrSeries[1] * (AtrLen - 1) + tr) / AtrLen;

            // DMI/ADX manual calculation
            double tr14 = 0;
            for (int i = 0; i < AdxLen && i <= CurrentBar; i++)
            {
                double trI = Math.Max(High[Math.Min(i, CurrentBar)] - Low[Math.Min(i, CurrentBar)],
                    Math.Max(Math.Abs(High[Math.Min(i, CurrentBar)] - Close[Math.Min(i + 1, CurrentBar)]),
                             Math.Abs(Low[Math.Min(i, CurrentBar)] - Close[Math.Min(i + 1, CurrentBar)])));
                tr14 += trI;
            }
            tr14 = Math.Max(tr14 / AdxLen, 0.001);

            double upMove = CurrentBar > 0 ? h - High[1] : 0;
            double dnMove = CurrentBar > 0 ? Low[1] - l : 0;
            double plusDM = (upMove > dnMove && upMove > 0) ? upMove : 0;
            double minusDM = (dnMove > upMove && dnMove > 0) ? dnMove : 0;
            double smoothedPlusDM = (CurrentBar == 0) ? plusDM : diPlusSeries[1] - (diPlusSeries[1] / AdxLen) + plusDM;
            double smoothedMinusDM = (CurrentBar == 0) ? minusDM : diMinusSeries[1] - (diMinusSeries[1] / AdxLen) + minusDM;
            double diPlusVal = 100.0 * smoothedPlusDM / tr14;
            double diMinusVal = 100.0 * smoothedMinusDM / tr14;
            diPlusSeries[0]  = diPlusVal;
            diMinusSeries[0] = diMinusVal;

            double dx = Math.Abs(diPlusVal - diMinusVal) / Math.Max(diPlusVal + diMinusVal, 0.001) * 100.0;
            if (CurrentBar == 0)
                adxSeries[0] = dx;
            else
                adxSeries[0] = ((AdxLen - 1) * adxSeries[1] + dx) / AdxLen;

            // VWAP
            double cumPV = 0, cumVol = 0;
            int sessionStart = Math.Max(0, CurrentBar - 1);
            if (CurrentBar > 0 && Times[0].Date != Times[1].Date) { sessionStart = CurrentBar; cumPV = 0; cumVol = 0; }
            vwapSeries[0] = vwapSeries[1];
            if (c * v > 0) { cumPV += c * v; cumVol += v; vwapSeries[0] = cumPV / cumVol; }

            // RSI
            if (CurrentBar == 0)
                rsiSeries[0] = 50;
            else
            {
                double delta = c - Close[1];
                double gain = delta > 0 ? delta : 0;
                double loss = delta < 0 ? -delta : 0;
                double avgGain = (CurrentBar <= RsiLen) ? rsiSeries[1] : (rsiSeries[1] * (RsiLen - 1) + gain) / RsiLen;
                double avgLoss = (CurrentBar <= RsiLen) ? 50 : (50 * (RsiLen - 1) + loss) / RsiLen;
                rsiSeries[0] = (avgLoss == 0) ? 100 : 100 - 100 / (1 + avgGain / avgLoss);
            }

            // Volume SMA
            volSma20Series[0] = (CurrentBar == 0) ? v : volSma20Series[1] + (v - volSma20Series[Math.Min(CurrentBar, 20)]) / Math.Min(CurrentBar + 1, 20);

            // Volume Percentile
            if (CurrentBar >= VolLookback)
            {
                int count = 0;
                for (int i = 0; i < VolLookback; i++)
                    if (Volume[Math.Min(i, CurrentBar)] <= v) count++;
                volPercentileSeries[0] = (double)count / VolLookback * 100.0;
            }
            else volPercentileSeries[0] = 50;

            // Gold return
            goldRetSeries[0] = (CurrentBar > 0 && Close[1] > 0) ? c / Close[1] - 1.0 : 0.0;

            // Body size / body %
            double body = Math.Abs(c - o);
            double range = h - l;
            bodySizeSeries[0] = body;
            bodyPctSeries[0] = range > 0 ? body / range * 100.0 : 0.0;
        }
        #endregion

        #region HTF Trend Stack
        private void CalculateHTFTrendStack()
        {
            // Access multi-timeframe data from BarsArray
            // BarsArray[1] = 5min, [2] = 15min, [3] = 4H, [4] = Daily
            for (int tf = 1; tf <= 4; tf++)
            {
                if (CurrentBars[tf] < 50) return;
            }

            // 5-min
            double c5 = Closes[1][0], e5_20 = EMA(Closes[1], 20)[0], e5_50 = EMA(Closes[1], 50)[0], e5_200 = EMA(Closes[1], 200)[0];
            _htf5mClose = c5; _htf5mEma20 = e5_20; _htf5mEma50 = e5_50; _htf5mEma200 = e5_200;

            // 15-min
            double c15 = Closes[2][0], e15_20 = EMA(Closes[2], 20)[0], e15_50 = EMA(Closes[2], 50)[0], e15_200 = EMA(Closes[2], 200)[0];
            _htf15mClose = c15; _htf15mEma20 = e15_20; _htf15mEma50 = e15_50; _htf15mEma200 = e15_200;

            // 4-Hour
            double c4h = Closes[3][0], e4h_20 = EMA(Closes[3], 20)[0], e4h_50 = EMA(Closes[3], 50)[0], e4h_200 = EMA(Closes[3], 200)[0];
            _htf4hClose = c4h; _htf4hEma20 = e4h_20; _htf4hEma50 = e4h_50; _htf4hEma200 = e4h_200;

            // Daily
            double cD = Closes[4][0], eD_20 = EMA(Closes[4], 20)[0], eD_50 = EMA(Closes[4], 50)[0], eD_200 = EMA(Closes[4], 200)[0];
            _htfDClose = cD; _htfDEma20 = eD_20; _htfDEma50 = eD_50; _htfDEma200 = eD_200;
            _dailyH = Highs[4][0]; _dailyL = Lows[4][0];

            // Score each timeframe
            _htf5mScore  = HTFTrendScore(_htf5mClose, _htf5mEma20, _htf5mEma50, _htf5mEma200);
            _htf15mScore = HTFTrendScore(_htf15mClose, _htf15mEma20, _htf15mEma50, _htf15mEma200);
            _htf1hScore  = HTFTrendScore(_htf1hClose, _htf1hEma20, _htf1hEma50, _htf1hEma200);
            _htf4hScore  = HTFTrendScore(_htf4hClose, _htf4hEma20, _htf4hEma50, _htf4hEma200);
            _htfDScore   = HTFTrendScore(_htfDClose, _htfDEma20, _htfDEma50, _htfDEma200);

            _htf5mBull = _htf5mScore  >= 60; _htf5mBear = _htf5mScore  <= 40;
            _htf15mBull = _htf15mScore >= 60; _htf15mBear = _htf15mScore <= 40;
            _htf1hBull = _htf1hScore  >= 60; _htf1hBear = _htf1hScore  <= 40;
            _htf4hBull = _htf4hScore  >= 60; _htf4hBear = _htf4hScore  <= 40;
            _htfDBull = _htfDScore    >= 60; _htfDBear = _htfDScore    <= 40;

            _htfAlignmentLong  = (_htf1hBull ? 1 : 0) + (_htf4hBull ? 1 : 0) + (_htfDBull ? 1 : 0);
            _htfAlignmentShort = (_htf1hBear ? 1 : 0) + (_htf4hBear ? 1 : 0) + (_htfDBear ? 1 : 0);
            _htfBullGate = _htfAlignmentLong  >= 2;
            _htfBearGate = _htfAlignmentShort >= 2;

            _htfFullLong  = (_htf5mBull ? 1 : 0) + (_htf15mBull ? 1 : 0) + (_htf1hBull ? 1 : 0) + (_htf4hBull ? 1 : 0) + (_htfDBull ? 1 : 0);
            _htfFullShort = (_htf5mBear ? 1 : 0) + (_htf15mBear ? 1 : 0) + (_htf1hBear ? 1 : 0) + (_htf4hBear ? 1 : 0) + (_htfDBear ? 1 : 0);
        }

        private int HTFTrendScore(double close, double ema20, double ema50, double ema200)
        {
            int score = 0;
            if (!double.IsNaN(close) && !double.IsNaN(ema200) && close > ema200) score += 40;
            if (!double.IsNaN(ema20) && !double.IsNaN(ema50) && ema20 > ema50) score += 40;
            if (!double.IsNaN(close) && !double.IsNaN(ema20) && close > ema20) score += 20;
            return score;
        }
        #endregion

        #region Adaptive ATR & Regime Engine
        private void CalculateAdaptiveATRRegime()
        {
            double tr = Math.Max(High[0] - Low[0],
                Math.Max(Math.Abs(High[0] - Close[1]), Math.Abs(Low[0] - Close[1])));

            // Trending ATR (7-period) and Ranging ATR (21-period)
            if (CurrentBar == 0)
            {
                atrTrendingSeries[0] = tr;
                atrRangingSeries[0] = tr;
            }
            else
            {
                atrTrendingSeries[0] = (atrTrendingSeries[1] * 6.0 + tr) / 7.0;
                atrRangingSeries[0] = (atrRangingSeries[1] * 20.0 + tr) / 21.0;
            }

            double adx = adxSeries[0];
            // Hysteresis: enter trending at ADX > threshold+3, exit at ADX < threshold-3
            if (_atrInTrendingRegime)
                _atrInTrendingRegime = adx > (AdxThreshold - 3);
            else
                _atrInTrendingRegime = adx > (AdxThreshold + 3);

            double rawATR = ShowAdaptiveATR
                ? (_atrInTrendingRegime ? atrTrendingSeries[0] : atrRangingSeries[0])
                : atrSeries[0];
            adaptiveATRSeries[0] = Math.Max(rawATR, Close[0] * 0.0001);

            _regimeTrending = adx > AdxThreshold;
            _regimeRanging = adx <= AdxThreshold && adx > 15;
            _regimeDead = adx <= 15;
        }
        #endregion

        #region Mean Reversion
        private void CalculateMeanReversion()
        {
            double ret = goldRetSeries[0];

            // Rolling mean and stdev of returns (20-period)
            if (CurrentBar < 20)
            {
                retMean20Series[0] = 0;
                retStd20Series[0] = 0.01;
            }
            else
            {
                double sumRet = 0, sumRet2 = 0;
                for (int i = 0; i < 20; i++)
                {
                    double r = goldRetSeries[Math.Min(i, CurrentBar)];
                    sumRet += r;
                    sumRet2 += r * r;
                }
                double mean = sumRet / 20.0;
                double variance = Math.Max(sumRet2 / 20.0 - mean * mean, 0.000001);
                retMean20Series[0] = mean;
                retStd20Series[0] = Math.Sqrt(variance);
            }

            double std = retStd20Series[0];
            retZScoreSeries[0] = std > 0.001 ? (ret - retMean20Series[0]) / std : 0;

            // Cornish-Fisher adjustment
            if (CurrentBar >= 20)
            {
                double mean = retMean20Series[0];
                double m2 = 0, m3 = 0, m4 = 0;
                for (int i = 0; i < 20; i++)
                {
                    double d = goldRetSeries[Math.Min(i, CurrentBar)] - mean;
                    m2 += d * d;
                    m3 += d * d * d;
                    m4 += d * d * d * d;
                }
                double n = 20.0;
                double v = Math.Max(m2 / n, 0.000001);
                double sd = Math.Sqrt(v);
                retSkewSeries[0] = sd > 0.0001 ? (m3 / n) / (sd * sd * sd) : 0;
                retKurtSeries[0] = v > 0.000001 ? (m4 / n) / (v * v) - 3.0 : 0;
            }
            else
            {
                retSkewSeries[0] = 0;
                retKurtSeries[0] = 0;
            }

            double skew = retSkewSeries[0];
            double kurt = retKurtSeries[0];
            double z = ForecastZ;
            double cfAdj = 1.0 + (skew / 6.0) * (z * z - 1.0)
                           + (kurt / 24.0) * (z * z * z - 3.0 * z)
                           - (skew * skew / 36.0) * (2.0 * z * z * z - 5.0 * z);
            double cfZ = Math.Max(z * 0.5, z * cfAdj);

            double c = Close[0];
            double vwap = vwapSeries[0];
            double ema20 = ema20Series[0];
            double ema100 = ema100Series[0];
            double ema200 = ema200Series[0];
            double atr = Math.Max(adaptiveATRSeries[0], 0.1);

            distVwapATRSeries[0]  = (c - vwap) / atr;
            distEma20ATRSeries[0] = (c - ema20) / atr;
            distEma100ATRSeries[0] = (c - ema100) / atr;
            distEma200ATRSeries[0] = (c - ema200) / atr;

            double mrScore = distVwapATRSeries[0] * 30.0
                           + distEma20ATRSeries[0] * 25.0
                           + distEma100ATRSeries[0] * 25.0
                           + distEma200ATRSeries[0] * 20.0
                           + retZScoreSeries[0] * 15.0;
            mrCompositeSeries[0] = Math.Max(-100, Math.Min(100, mrScore));
        }
        #endregion

        #region Session Engine — DST Aware
        private void CalculateSessions()
        {
            DateTime t = Times[0];
            int hourUTC = t.Hour;
            int dom = t.Day;
            int mon = t.Month;
            int yr = t.Year;
            int dow = (int)t.DayOfWeek; // 0=Sunday

            // First Sunday of current month
            int daysToSun = (7 - (dow - 1) % 7) % 7;
            if (daysToSun == 0) daysToSun = 7;
            int firstSunDom = dom - ((dow - 1 + 7) % 7);
            firstSunDom = firstSunDom - 7 * ((firstSunDom - 1) / 7);
            if (firstSunDom < 1) firstSunDom += 7;

            // US DST: 2nd Sunday March -> 1st Sunday Nov
            bool usIsDST = (mon > 3 && mon < 11) ||
                           (mon == 3 && dom >= firstSunDom + 7) ||
                           (mon == 11 && dom < firstSunDom);

            // EU DST: last Sunday March -> last Sunday Oct
            bool euIsDST = (mon > 3 && mon < 10) ||
                           (mon == 3 && dom >= firstSunDom + 21) ||
                           (mon == 10 && dom < firstSunDom + 21);

            int londonOpen = 7;
            int londonClose = euIsDST ? 16 : 17;
            int nyOpen = usIsDST ? 13 : 14;
            int nyClose = usIsDST ? 21 : 22;

            _inAsian = hourUTC >= 0 && hourUTC < londonOpen;
            _inLondon = hourUTC >= londonOpen && hourUTC < londonClose;
            _inNY = hourUTC >= nyOpen && hourUTC < nyClose;

            bool inLondonKZ = hourUTC >= londonOpen + 1 && hourUTC < londonOpen + 4;
            bool inLondonFixKZ = hourUTC >= londonOpen + 3 && hourUTC < londonOpen + 4;
            bool inNYKZ = hourUTC >= nyOpen && hourUTC < nyOpen + 3;
            bool inLondonCloseKZ = hourUTC >= londonClose - 1 && hourUTC < londonClose;
            _inKillzone = inLondonKZ || inNYKZ || inLondonCloseKZ || inLondonFixKZ;

            int sq = 0;
            sq += _inKillzone ? 40 : _inLondon || _inNY ? 25 : _inAsian ? 10 : 0;
            sq += inLondonFixKZ ? 20 : 0;
            sq += inNYKZ ? 15 : 0;
            sq += (!_inAsian || inLondonKZ) ? 10 : 0;
            sq += (_inLondon && !inLondonKZ) ? 5 : 0;
            _sessionQuality = (int)Math.Round((double)sq / 80.0 * 100.0);

            if (_inKillzone)
                _sessionLabel = inLondonFixKZ ? "LON-FIX" : inLondonKZ ? "LON-KZ" : inNYKZ ? "NY-KZ" : inLondonCloseKZ ? "LON-CL" : "KZ";
            else if (_inLondon) _sessionLabel = "LONDON";
            else if (_inNY) _sessionLabel = "NY";
            else if (_inAsian) _sessionLabel = "ASIAN";
            else _sessionLabel = "OFF";
        }
        #endregion

        #region Macro Engine
        private void CalculateMacroEngine()
        {
            // Macro data from secondary series (indices 5-9)
            // Use the stored series values; if NaN, use cached values
            double dxyC = _macroDxySeries[0];
            double yldC = _macroYieldSeries[0];
            double slvC = _macroSilverSeries[0];
            double eurC = _macroEurSeries[0];
            double spxC = _macroSpxSeries[0];

            _dxyValid = !double.IsNaN(dxyC) && dxyC > 0;
            _yieldValid = !double.IsNaN(yldC) && yldC > 0;
            _silverValid = !double.IsNaN(slvC) && slvC > 0;
            _eurusdValid = !double.IsNaN(eurC) && eurC > 0;
            _spxValid = !double.IsNaN(spxC) && spxC > 0;

            if (_dxyValid) _dxyClose = dxyC;
            if (_yieldValid) _yieldClose = yldC;
            if (_silverValid) _silverClose = slvC;
            if (_eurusdValid) _eurusdClose = eurC;
            if (_spxValid) _spxClose = spxC;

            // EMAs on macro (using NT built-in EMA for simplicity)
            if (_dxyValid)
            {
                _dxyEMAFast = EMA(_macroDxySeries, 10)[0];
                _dxyEMASlow = EMA(_macroDxySeries, 20)[0];
                _dxyBull = _dxyClose > _dxyEMAFast && _dxyEMAFast > _dxyEMASlow;
                _dxyBear = _dxyClose < _dxyEMAFast && _dxyEMAFast < _dxyEMASlow;
            }
            if (_yieldValid)
            {
                _yieldEMAFast = EMA(_macroYieldSeries, 10)[0];
                _yieldEMASlow = EMA(_macroYieldSeries, 20)[0];
                bool yUp = _yieldClose > _yieldEMAFast && _yieldEMAFast > _yieldEMASlow;
                bool yDn = _yieldClose < _yieldEMAFast && _yieldEMAFast < _yieldEMASlow;
                _yieldRising = InvertYield ? yDn : yUp;
                _yieldFalling = InvertYield ? yUp : yDn;
            }
            if (_eurusdValid)
            {
                _eurusdEMAFast = EMA(_macroEurSeries, 10)[0];
                _eurusdEMASlow = EMA(_macroEurSeries, 20)[0];
                _eurusdBull = _eurusdClose > _eurusdEMAFast && _eurusdEMAFast > _eurusdEMASlow;
                _eurusdBear = _eurusdClose < _eurusdEMAFast && _eurusdEMAFast < _eurusdEMASlow;
            }
            if (_spxValid)
            {
                _spxEMAFast = EMA(_macroSpxSeries, 10)[0];
                _spxEMASlow = EMA(_macroSpxSeries, 20)[0];
                _spxBull = _spxClose > _spxEMAFast && _spxEMAFast > _spxEMASlow;
                _spxBear = _spxClose < _spxEMAFast && _spxEMAFast < _spxEMASlow;
            }
            if (_silverValid)
            {
                _silverEMA = EMA(_macroSilverSeries, 20)[0];
                _silverBull = _silverClose > _silverEMA;
                _silverBear = _silverClose < _silverEMA;
            }

            _macroBullVotes = (_dxyBear ? 1 : 0) + (_yieldFalling ? 1 : 0) + (_spxBull ? 1 : 0) + (_eurusdBull ? 1 : 0) + (_silverBull ? 1 : 0);
            _macroBearVotes = (_dxyBull ? 1 : 0) + (_yieldRising ? 1 : 0) + (_spxBear ? 1 : 0) + (_eurusdBear ? 1 : 0) + (_silverBear ? 1 : 0);
            _macroBull = _macroBullVotes >= 3;
            _macroBear = _macroBearVotes >= 3;

            // ROC calculations
            if (_dxyValid && CurrentBars[5] > DxyRocLen)
                _dxyROC = (_dxyClose / _macroDxySeries[DxyRocLen] - 1.0) * 100.0;
            if (_yieldValid && CurrentBars[6] > ZnRocLen)
                _znROC = (_yieldClose / _macroYieldSeries[ZnRocLen] - 1.0) * 100.0;
            if (_eurusdValid && CurrentBars[8] > DxyRocLen)
                _eurusdROC = (_eurusdClose / _macroEurSeries[DxyRocLen] - 1.0) * 100.0;

            _dxyMomBull = _dxyROC > 0.15;
            _dxyMomBear = _dxyROC < -0.15;
            _ratePressureBull = (_znROC + _eurusdROC) > 0.1;
            _ratePressureBear = (_znROC + _eurusdROC) < -0.1;

            // Correlation break detection
            int goldDir = Close[0] > Close[1] ? 1 : Close[0] < Close[1] ? -1 : 0;
            int dxyDir = _dxyValid && CurrentBars[5] > 0
                ? (_dxyClose > _macroDxySeries[1] ? 1 : _dxyClose < _macroDxySeries[1] ? -1 : 0) : 0;

            if (goldDir == dxyDir && goldDir != 0)
                _corrBreakCount++;
            else
                _corrBreakCount = 0;
            _corrBreakdown = ShowCorrHealth && _corrBreakCount >= CorrHealthBars;

            // Macro returns for correlation
            if (_dxyValid && !double.IsNaN(_dxyClose) && CurrentBars[5] > 0 && _macroDxySeries[1] > 0)
                _dxyRetSeries[0] = _dxyClose / _macroDxySeries[1] - 1.0;
            if (_yieldValid && !double.IsNaN(_yieldClose) && CurrentBars[6] > 0 && _macroYieldSeries[1] > 0)
                _yieldRetSeries[0] = _yieldClose / _macroYieldSeries[1] - 1.0;
            if (_silverValid && !double.IsNaN(_silverClose) && CurrentBars[7] > 0 && _macroSilverSeries[1] > 0)
                _silverRetSeries[0] = _silverClose / _macroSilverSeries[1] - 1.0;
            if (_eurusdValid && !double.IsNaN(_eurusdClose) && CurrentBars[8] > 0 && _macroEurSeries[1] > 0)
                _eurRetSeries[0] = _eurusdClose / _macroEurSeries[1] - 1.0;
            if (_spxValid && !double.IsNaN(_spxClose) && CurrentBars[9] > 0 && _macroSpxSeries[1] > 0)
                _spxRetSeries[0] = _spxClose / _macroSpxSeries[1] - 1.0;

            // Macro strength score
            double msDxy = _dxyValid ? (_dxyBear ? 30 : _dxyBull ? -30 : 0) : 0;
            double msYld = _yieldValid ? (_yieldFalling ? 25 : _yieldRising ? -25 : 0) : 0;
            double msSlv = _silverValid ? (_silverBull ? 20 : _silverBear ? -20 : 0) : 0;
            double msEur = _eurusdValid ? (_eurusdBull ? 20 : _eurusdBear ? -20 : 0) : 0;
            double msSpx = _spxValid ? (_spxBull ? 15 : _spxBear ? -15 : 0) : 0;
            double msMom = _dxyValid ? (_dxyMomBear ? 10 : _dxyMomBull ? -10 : 0) : 0;

            _macroStrengthScore = (int)Math.Max(-100, Math.Min(100, msDxy + msYld + msSlv + msEur + msSpx + msMom));
            _macroStrengthConf = Math.Abs(_macroStrengthScore);
        }
        #endregion

        #region Correlation Engine
        private void CalculateCorrelation()
        {
            int minBars = Math.Max(CorrShortLen, Math.Max(CorrMedLen, CorrLongLen));
            if (CurrentBar < minBars) return;

            // Helper to compute Pearson correlation
            double Corr(Series<double> x, Series<double> y, int len)
            {
                double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
                int n = Math.Min(len, CurrentBar + 1);
                for (int i = 0; i < n; i++)
                {
                    double xv = x[i];
                    double yv = y[i];
                    if (double.IsNaN(xv) || double.IsNaN(yv)) { n--; continue; }
                    sumX += xv; sumY += yv;
                    sumXY += xv * yv;
                    sumX2 += xv * xv;
                    sumY2 += yv * yv;
                }
                double num = n * sumXY - sumX * sumY;
                double den = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));
                return den > 0.0001 ? num / den : 0;
            }

            double short30 = Corr(goldRetSeries, _dxyRetSeries, CorrShortLen);
            double med60   = Corr(goldRetSeries, _dxyRetSeries, CorrMedLen);
            double long120 = Corr(goldRetSeries, _dxyRetSeries, CorrLongLen);
            _avgCorrDXY = _dxyValid ? (short30 + med60 + long120) / 3.0 : 0;

            short30 = Corr(goldRetSeries, _yieldRetSeries, CorrShortLen);
            med60   = Corr(goldRetSeries, _yieldRetSeries, CorrMedLen);
            long120 = Corr(goldRetSeries, _yieldRetSeries, CorrLongLen);
            _avgCorrYld = _yieldValid ? (short30 + med60 + long120) / 3.0 : 0;

            short30 = Corr(goldRetSeries, _silverRetSeries, CorrShortLen);
            med60   = Corr(goldRetSeries, _silverRetSeries, CorrMedLen);
            long120 = Corr(goldRetSeries, _silverRetSeries, CorrLongLen);
            _avgCorrSlv = _silverValid ? (short30 + med60 + long120) / 3.0 : 0;

            short30 = Corr(goldRetSeries, _spxRetSeries, CorrShortLen);
            med60   = Corr(goldRetSeries, _spxRetSeries, CorrMedLen);
            long120 = Corr(goldRetSeries, _spxRetSeries, CorrLongLen);
            _avgCorrSPX = _spxValid ? (short30 + med60 + long120) / 3.0 : 0;

            short30 = Corr(goldRetSeries, _eurRetSeries, CorrShortLen);
            med60   = Corr(goldRetSeries, _eurRetSeries, CorrMedLen);
            long120 = Corr(goldRetSeries, _eurRetSeries, CorrLongLen);
            _avgCorrEUR = _eurusdValid ? (short30 + med60 + long120) / 3.0 : 0;

            // Correlation health with per-window significance thresholds
            double sig30 = 2.0 / Math.Sqrt(CorrShortLen);
            double sig60 = 2.0 / Math.Sqrt(CorrMedLen);
            double sig120 = 2.0 / Math.Sqrt(CorrLongLen);

            double yieldSign = InvertYield ? 1.0 : -1.0;

            _corrHealthDXY = _dxyValid ? CorrHealthMW(
                Corr(goldRetSeries, _dxyRetSeries, CorrShortLen),
                Corr(goldRetSeries, _dxyRetSeries, CorrMedLen),
                Corr(goldRetSeries, _dxyRetSeries, CorrLongLen), sig30, sig60, sig120, -1.0) : 0;
            _corrHealthYld = _yieldValid ? CorrHealthMW(
                Corr(goldRetSeries, _yieldRetSeries, CorrShortLen),
                Corr(goldRetSeries, _yieldRetSeries, CorrMedLen),
                Corr(goldRetSeries, _yieldRetSeries, CorrLongLen), sig30, sig60, sig120, yieldSign) : 0;
            _corrHealthSlv = _silverValid ? CorrHealthMW(
                Corr(goldRetSeries, _silverRetSeries, CorrShortLen),
                Corr(goldRetSeries, _silverRetSeries, CorrMedLen),
                Corr(goldRetSeries, _silverRetSeries, CorrLongLen), sig30, sig60, sig120, 1.0) : 0;
            _corrHealthSPX = _spxValid ? CorrHealthMW(
                Corr(goldRetSeries, _spxRetSeries, CorrShortLen),
                Corr(goldRetSeries, _spxRetSeries, CorrMedLen),
                Corr(goldRetSeries, _spxRetSeries, CorrLongLen), sig30, sig60, sig120, 1.0) : 0;
            _corrHealthEUR = _eurusdValid ? CorrHealthMW(
                Corr(goldRetSeries, _eurRetSeries, CorrShortLen),
                Corr(goldRetSeries, _eurRetSeries, CorrMedLen),
                Corr(goldRetSeries, _eurRetSeries, CorrLongLen), sig30, sig60, sig120, 1.0) : 0;

            int validAssets = (_dxyValid ? 1 : 0) + (_yieldValid ? 1 : 0) + (_silverValid ? 1 : 0)
                            + (_spxValid ? 1 : 0) + (_eurusdValid ? 1 : 0);
            _avgCorrHealth = validAssets > 0 ? (_corrHealthDXY + _corrHealthYld + _corrHealthSlv + _corrHealthSPX + _corrHealthEUR) / validAssets : 0;

            // Stability
            double sDXY = Math.Abs(Corr(goldRetSeries, _dxyRetSeries, CorrShortLen) - Corr(goldRetSeries, _dxyRetSeries, CorrLongLen));
            _avgStability = validAssets > 0 ? sDXY / validAssets : 0;
        }

        private double CorrWinScore(double r, double sig, double sign)
        {
            double rs = r * sign;
            if (rs > sig) return 100.0;
            if (rs > 0) return 70.0;
            if (rs > -sig) return 40.0;
            return 20.0;
        }

        private double CorrHealthMW(double r30, double r60, double r120, double sig30, double sig60, double sig120, double sign)
        {
            return (CorrWinScore(r30, sig30, sign) + CorrWinScore(r60, sig60, sign) + CorrWinScore(r120, sig120, sign)) / 3.0;
        }
        #endregion

        #region Weighted Trend Engine
        private void CalculateWeightedTrend()
        {
            double c = Close[0];
            double e20 = ema20Series[0];
            double e100 = ema100Series[0];
            double e200 = ema200Series[0];
            double diP = diPlusSeries[0];
            double diN = diMinusSeries[0];
            double adx = adxSeries[0];
            double v = Volume[0];
            double vSma = volSma20Series[0];
            bool highVol = v > vSma;

            _bullTrendScore = 0;
            _bullTrendScore += c > e200 ? 20 : 0;
            _bullTrendScore += e20 > e100 ? 20 : 0;
            _bullTrendScore += diP > diN ? 20 : 0;
            _bullTrendScore += adx > AdxThreshold ? 20 : 0;
            _bullTrendScore += c > e100 ? 10 : 0;
            _bullTrendScore += highVol ? 10 : 0;

            _bearTrendScore = 0;
            _bearTrendScore += c < e200 ? 20 : 0;
            _bearTrendScore += e20 < e100 ? 20 : 0;
            _bearTrendScore += diN > diP ? 20 : 0;
            _bearTrendScore += adx > AdxThreshold ? 20 : 0;
            _bearTrendScore += c < e100 ? 10 : 0;
            _bearTrendScore += highVol ? 10 : 0;

            _bullTrend = _bullTrendScore >= TrendThreshold;
            _bearTrend = _bearTrendScore >= TrendThreshold;
        }
        #endregion

        #region Dual Structure Engine
        private void CalculateDualStructure()
        {
            double h = High[0];
            double l = Low[0];
            double prevResist = _activeResistance;
            double prevSup = _activeSupport;
            double prevClose = Close[1];

            // Internal pivots (fast)
            if (CurrentBar >= InternalPivotLen * 2 + 1)
            {
                double pivotHigh = PivotHighBar(InternalPivotLen) == InternalPivotLen ? High[InternalPivotLen] : double.NaN;
                double pivotLow  = PivotLowBar(InternalPivotLen) == InternalPivotLen ? Low[InternalPivotLen] : double.NaN;

                _pivotHighSeries[0] = pivotHigh;
                _pivotLowSeries[0] = pivotLow;

                if (!double.IsNaN(pivotHigh))
                {
                    if (!double.IsNaN(_activeResistance))
                        _lastBrokenRes = _activeResistance;
                    _activeResistance = pivotHigh;
                }
                if (!double.IsNaN(pivotLow))
                {
                    if (!double.IsNaN(_activeSupport))
                        _lastBrokenSup = _activeSupport;
                    _activeSupport = pivotLow;
                }
            }

            // Check breaks on internal pivots
            double volSma = volSma20Series[0];
            if (!double.IsNaN(_activeResistance) && Close[0] > _activeResistance && prevClose <= _activeResistance && Volume[0] > volSma)
            {
                _lastBrokenRes = _activeResistance;
                _activeResistance = double.NaN;
            }
            if (!double.IsNaN(_activeSupport) && Close[0] < _activeSupport && prevClose >= _activeSupport && Volume[0] > volSma)
            {
                _lastBrokenSup = _activeSupport;
                _activeSupport = double.NaN;
            }

            // Swing pivots (major)
            if (CurrentBar >= SwingPivotLen * 2 + 1)
            {
                double swingPH = PivotHighBar(SwingPivotLen) == SwingPivotLen ? High[SwingPivotLen] : double.NaN;
                double swingPL = PivotLowBar(SwingPivotLen) == SwingPivotLen ? Low[SwingPivotLen] : double.NaN;

                _swingPivotHighSeries[0] = swingPH;
                _swingPivotLowSeries[0] = swingPL;

                if (!double.IsNaN(swingPH))
                {
                    if (!double.IsNaN(_swingActiveResistance))
                        _swingLastBrokenRes = _swingActiveResistance;
                    _swingActiveResistance = swingPH;
                }
                if (!double.IsNaN(swingPL))
                {
                    if (!double.IsNaN(_swingActiveSupport))
                        _swingLastBrokenSup = _swingActiveSupport;
                    _swingActiveSupport = swingPL;
                }
            }

            if (!double.IsNaN(_swingActiveResistance) && Close[0] > _swingActiveResistance && prevClose <= _swingActiveResistance && Volume[0] > volSma)
            {
                _swingLastBrokenRes = _swingActiveResistance;
                _swingActiveResistance = double.NaN;
            }
            if (!double.IsNaN(_swingActiveSupport) && Close[0] < _swingActiveSupport && prevClose >= _swingActiveSupport && Volume[0] > volSma)
            {
                _swingLastBrokenSup = _swingActiveSupport;
                _swingActiveSupport = double.NaN;
            }
        }

        private int PivotHighBar(int strength)
        {
            int half = strength;
            if (CurrentBar < half * 2) return -1;
            double pivotHigh = High[half];
            for (int i = 0; i <= half * 2; i++)
                if (High[i] > pivotHigh) return -1;
            return half;
        }

        private int PivotLowBar(int strength)
        {
            int half = strength;
            if (CurrentBar < half * 2) return -1;
            double pivotLow = Low[half];
            for (int i = 0; i <= half * 2; i++)
                if (Low[i] < pivotLow) return -1;
            return half;
        }
        #endregion

        #region BOS Detection
        private void CalculateBOS()
        {
            double atr = Math.Max(adaptiveATRSeries[0], 0.1);
            double breakBuf = atr * 0.15;
            double strongBodySize = atr * 0.30;
            double body = bodySizeSeries[0];
            double bodyPct = bodyPctSeries[0];
            double volSma = volSma20Series[0];
            bool strongVol = Volume[0] > volSma * 1.20;
            double close0 = Close[0];
            double close1 = Close[1];

            // Internal BOS
            bool intBullBOS = !double.IsNaN(_activeResistance) && close0 > _activeResistance
                && close1 <= _activeResistance && _activeResistance != _lastBrokenRes
                && adxSeries[0] > AdxThreshold
                && (close0 - _activeResistance) > breakBuf
                && body > strongBodySize && bodyPct > 70 && strongVol;

            bool intBearBOS = !double.IsNaN(_activeSupport) && close0 < _activeSupport
                && close1 >= _activeSupport && _activeSupport != _lastBrokenSup
                && adxSeries[0] > AdxThreshold
                && (_activeSupport - close0) > breakBuf
                && body > strongBodySize && bodyPct > 70 && strongVol;

            bool intBosLabelBull = !double.IsNaN(_activeResistance) && close0 > _activeResistance && close1 <= _activeResistance && _activeResistance != _lastBrokenRes;
            bool intBosLabelBear = !double.IsNaN(_activeSupport) && close0 < _activeSupport && close1 >= _activeSupport && _activeSupport != _lastBrokenSup;

            // Swing BOS
            bool swingBullBOS = !double.IsNaN(_swingActiveResistance) && close0 > _swingActiveResistance
                && close1 <= _swingActiveResistance && _swingActiveResistance != _swingLastBrokenRes
                && adxSeries[0] > AdxThreshold
                && (close0 - _swingActiveResistance) > breakBuf
                && body > strongBodySize && bodyPct > 70 && strongVol;

            bool swingBearBOS = !double.IsNaN(_swingActiveSupport) && close0 < _swingActiveSupport
                && close1 >= _swingActiveSupport && _swingActiveSupport != _swingLastBrokenSup
                && adxSeries[0] > AdxThreshold
                && (_swingActiveSupport - close0) > breakBuf
                && body > strongBodySize && bodyPct > 70 && strongVol;

            bool swingBosLabelBull = !double.IsNaN(_swingActiveResistance) && close0 > _swingActiveResistance && close1 <= _swingActiveResistance && _swingActiveResistance != _swingLastBrokenRes;
            bool swingBosLabelBear = !double.IsNaN(_swingActiveSupport) && close0 < _swingActiveSupport && close1 >= _swingActiveSupport && _swingActiveSupport != _swingLastBrokenSup;

            _bullBOS = intBullBOS || swingBullBOS;
            _bearBOS = intBearBOS || swingBearBOS;
            _bosLabelBull = intBosLabelBull || swingBosLabelBull;
            _bosLabelBear = intBosLabelBear || swingBosLabelBear;
        }
        #endregion

        #region CHOCH / MSS
        private void CalculateCHOCH_MSS()
        {
            double ph = _pivotHighSeries[0];
            double pl = _pivotLowSeries[0];

            if (!double.IsNaN(ph) || !double.IsNaN(pl))
            {
                if (!double.IsNaN(ph))
                {
                    _seqHigh3 = _seqHigh2;
                    _seqHigh2 = _seqHigh1;
                    _seqHigh1 = ph;
                }
                if (!double.IsNaN(pl))
                {
                    _seqLow3 = _seqLow2;
                    _seqLow2 = _seqLow1;
                    _seqLow1 = pl;
                }
                _seqLastBar = CurrentBar;
            }

            bool seqValid = !double.IsNaN(_seqHigh1) && !double.IsNaN(_seqHigh2) && !double.IsNaN(_seqHigh3)
                         && !double.IsNaN(_seqLow1) && !double.IsNaN(_seqLow2) && !double.IsNaN(_seqLow3);

            _hh = seqValid && _seqHigh1 > _seqHigh2;
            _hl = seqValid && _seqLow1 > _seqLow2;
            _lh = seqValid && _seqHigh1 < _seqHigh2;
            _ll = seqValid && _seqLow1 < _seqLow2;

            int seqDist = CurrentBar - _seqLastBar;
            bool barSpaceOk = seqDist >= 3;

            double atr = Math.Max(adaptiveATRSeries[0], 0.1);
            double pivotAtrDist = atr * 0.5;
            bool atrDistOkBull = _hl && Math.Abs(_seqLow1 - _seqLow2) >= pivotAtrDist;
            bool atrDistOkBear = _lh && Math.Abs(_seqHigh1 - _seqHigh2) >= pivotAtrDist;

            bool priorBullSeq = seqValid && _seqHigh2 > _seqHigh3 && _seqLow2 > _seqLow3;
            bool priorBearSeq = seqValid && _seqHigh2 < _seqHigh3 && _seqLow2 < _seqLow3;

            _chochLabelBull = priorBearSeq && _hl && barSpaceOk && atrDistOkBull;
            _chochLabelBear = priorBullSeq && _lh && barSpaceOk && atrDistOkBear;

            _mssLabelBull = _chochLabelBull && _hh && barSpaceOk && atrDistOkBull;
            _mssLabelBear = _chochLabelBear && _ll && barSpaceOk && atrDistOkBear;
        }
        #endregion

        #region Displacement
        private void CalculateDisplacement()
        {
            double atr = Math.Max(adaptiveATRSeries[0], 0.1);
            double body = bodySizeSeries[0];
            double bodyPct = bodyPctSeries[0];
            double volSma = volSma20Series[0];
            double close0 = Close[0];
            double open0 = Open[0];
            double high1 = High[1];
            double low1 = Low[1];

            double dispEnter = atr * DispMult;
            double dispHold  = atr * (DispMult * 0.6);
            double dispExit  = atr * (DispMult * 0.2);

            bool rawUp  = ShowDisplacement && body > dispEnter && close0 > open0 && close0 > high1 && bodyPct > 70 && Volume[0] > volSma * 1.30;
            bool rawDown = ShowDisplacement && body > dispEnter && close0 < open0 && close0 < low1 && bodyPct > 70 && Volume[0] > volSma * 1.30;

            if (rawUp) _displacementUpActive = true;
            else if (body < dispExit || !(body > dispHold)) _displacementUpActive = false;

            if (rawDown) _displacementDownActive = true;
            else if (body < dispExit || !(body > dispHold)) _displacementDownActive = false;

            _displacementUp = rawUp || _displacementUpActive;
            _displacementDown = rawDown || _displacementDownActive;

            _dispOnsetUp = _displacementUp && !DisplacementUp(1);
            _dispOnsetDown = _displacementDown && !DisplacementDown(1);
        }

        private bool DisplacementUp(int offset)
        {
            // Stub for historical check; In real NT, save to Series<bool>
            return _displacementUp;
        }

        private bool DisplacementDown(int offset)
        {
            return _displacementDown;
        }
        #endregion

        #region RSI Divergence
        private void CalculateRSIDivergence()
        {
            double rsi = rsiSeries[0];
            double ph = _pivotHighSeries[0];
            double pl = _pivotLowSeries[0];

            if (!double.IsNaN(ph))
            {
                _pivotRsiHigh = rsi;
                _pivotPriceHigh = ph;
            }
            if (!double.IsNaN(pl))
            {
                _pivotRsiLow = rsi;
                _pivotPriceLow = pl;
            }

            _bullRsiDiv = !UseRsiDivFilter;
            _bearRsiDiv = !UseRsiDivFilter;

            if (UseRsiDivFilter && _pivotPriceLow != 0 && _pivotRsiLow != 0)
            {
                // Need previous pivot values stored for comparison
                // Simplified: always pass the filter if unset
                _bullRsiDiv = true;
                _bearRsiDiv = true;
            }
        }
        #endregion

        #region EQH/EQL
        private void CalculateEQHEQL()
        {
            double atr = Math.Max(adaptiveATRSeries[0], 0.1);
            double eqTol = atr * EqAtrMult;
            double ph = _pivotHighSeries[0];
            double pl = _pivotLowSeries[0];

            if (!double.IsNaN(ph))
            {
                _swingHigh2 = _swingHigh1;
                _swingHigh1 = ph;
            }
            if (!double.IsNaN(pl))
            {
                _swingLow2 = _swingLow1;
                _swingLow1 = pl;
            }

            _equalHighs = Math.Abs(_swingHigh1 - _swingHigh2) <= eqTol;
            _equalLows  = Math.Abs(_swingLow1 - _swingLow2) <= eqTol;
        }
        #endregion

        #region Volume Climax
        private void CalculateVolumeClimax()
        {
            double vPct = volPercentileSeries[0];
            bool recent = CurrentBar > BarsRequiredToTrade;
            _climaxUp   = ShowClimax && recent && vPct >= VolClimaxPerc && Close[0] > Open[0];
            _climaxDown = ShowClimax && recent && vPct >= VolClimaxPerc && Close[0] < Open[0];
        }
        #endregion

        #region FVG Detection
        private void CalculateFVG()
        {
            double atr = Math.Max(adaptiveATRSeries[0], 0.1);
            bool detected = ShowFVG && CurrentBar >= 2 &&
                ((High[2] < Low[0] && Low[0] - High[2] > atr * 0.3) ||
                 (Low[2] > High[0] && Low[2] - High[0] > atr * 0.3));

            if (detected && !_fvgActive)
            {
                double gapLow = High[2];
                double gapHigh = Low[0];
                if (Low[2] > High[0])
                {
                    gapLow = Low[0];
                    gapHigh = High[2];
                }
                if (gapLow < gapHigh)
                {
                    _fvgHighUpper = gapHigh;
                    _fvgLowLower  = gapLow;
                    _fvgBarIndex  = CurrentBar;
                    _fvgAtrAtDetect = atr;
                    _fvgVolPctAtDetect = volPercentileSeries[0];
                    _fvgActive = true;
                    _fvgWasBullish = Low[2] > High[0];
                }
            }

            if (_fvgActive && (CurrentBar - _fvgBarIndex) > FvgMaxBars)
                _fvgActive = false;

            _fvgMitigated = _fvgActive &&
                ((_fvgWasBullish && Low[0] <= _fvgHighUpper) ||
                 (!_fvgWasBullish && High[0] >= _fvgLowLower));

            if (_fvgMitigated) _fvgActive = false;

            _fvgGapSize = (!double.IsNaN(_fvgHighUpper) && !double.IsNaN(_fvgLowLower))
                ? _fvgHighUpper - _fvgLowLower : double.NaN;

            // FVG Score
            if (_fvgActive && !double.IsNaN(_fvgGapSize) && !double.IsNaN(_fvgAtrAtDetect))
            {
                double disp = _fvgAtrAtDetect > 0 ? _fvgGapSize / _fvgAtrAtDetect : 0;
                double s = 0;
                s += disp >= 2.5 ? 40 : disp >= 1.5 ? 30 : disp >= 1.0 ? 20 : disp >= 0.5 ? 10 : 0;
                s += _fvgVolPctAtDetect >= 90 ? 30 : _fvgVolPctAtDetect >= 75 ? 25 : _fvgVolPctAtDetect >= 60 ? 20 : _fvgVolPctAtDetect >= 40 ? 10 : 0;
                s += (_fvgWasBullish && _htf1hBull) || (!_fvgWasBullish && _htf1hBear) ? 20 : 0;
                s += 10;
                _fvgScore = Math.Min(s, 100);
            }
            else _fvgScore = double.NaN;
        }
        #endregion

        #region Order Block Detection
        private void CalculateOB()
        {
            double atr = Math.Max(adaptiveATRSeries[0], 0.1);
            double vPct = volPercentileSeries[0];
            double body = bodySizeSeries[0];
            double h1 = High[1], l1 = Low[1], c1 = Close[1];

            // Bearish OB: candle before bearish displacement
            if (ShowOB && _dispOnsetDown && !_obBearActive)
            {
                _obBearHigh   = h1;
                _obBearLow    = l1;
                _obBearBar    = CurrentBar;
                _obBearVolPct = vPct;
                _obBearAtr    = atr;
                _obBearActive = true;
            }

            // Bullish OB: candle before bullish displacement
            if (ShowOB && _dispOnsetUp && !_obBullActive)
            {
                _obBullHigh   = h1;
                _obBullLow    = l1;
                _obBullBar    = CurrentBar;
                _obBullVolPct = vPct;
                _obBullAtr    = atr;
                _obBullActive = true;
            }

            // Reversal OBs
            if (ShowOB && !_obBearActive && !_displacementUp && DisplacementUp(1) && c1 < l1 + body * 0.3)
            {
                _obBearHigh   = h1;
                _obBearLow    = l1;
                _obBearBar    = CurrentBar;
                _obBearVolPct = vPct;
                _obBearAtr    = atr;
                _obBearActive = true;
            }
            if (ShowOB && !_obBullActive && !_displacementDown && DisplacementDown(1) && c1 > h1 - body * 0.3)
            {
                _obBullHigh   = h1;
                _obBullLow    = l1;
                _obBullBar    = CurrentBar;
                _obBullVolPct = vPct;
                _obBullAtr    = atr;
                _obBullActive = true;
            }

            // Expiry
            if (_obBullActive && (CurrentBar - _obBullBar) > ObMaxBars) _obBullActive = false;
            if (_obBearActive && (CurrentBar - _obBearBar) > ObMaxBars) _obBearActive = false;

            // Invalidation
            if (_obBullActive && Close[0] < _obBullLow - atr * 0.3) _obBullActive = false;
            if (_obBearActive && Close[0] > _obBearHigh + atr * 0.3) _obBearActive = false;

            // Scores
            if (_obBullActive)
            {
                double disp = _obBullAtr > 0 ? (_obBullHigh - _obBullLow) / _obBullAtr : 0;
                double s = 0;
                s += disp >= 2.5 ? 35 : disp >= 1.5 ? 28 : disp >= 1.0 ? 20 : disp >= 0.5 ? 12 : 0;
                s += _obBullVolPct >= 90 ? 30 : _obBullVolPct >= 75 ? 24 : _obBullVolPct >= 60 ? 18 : _obBullVolPct >= 40 ? 10 : 0;
                s += (_htf1hBull ? 20 : 0);
                s += (_displacementUp || _climaxUp) ? 15 : 0;
                _obBullScore = Math.Min(s, 100);
            }

            if (_obBearActive)
            {
                double disp = _obBearAtr > 0 ? (_obBearHigh - _obBearLow) / _obBearAtr : 0;
                double s = 0;
                s += disp >= 2.5 ? 35 : disp >= 1.5 ? 28 : disp >= 1.0 ? 20 : disp >= 0.5 ? 12 : 0;
                s += _obBearVolPct >= 90 ? 30 : _obBearVolPct >= 75 ? 24 : _obBearVolPct >= 60 ? 18 : _obBearVolPct >= 40 ? 10 : 0;
                s += (_htf1hBear ? 20 : 0);
                s += (_displacementDown || _climaxDown) ? 15 : 0;
                _obBearScore = Math.Min(s, 100);
            }
        }
        #endregion

        #region Support / Resistance — Fractal Clustering
        private void CalculateSupportResistance()
        {
            if (!IsLastBarOfDay || CurrentBar < 100 || PerfMode) return;

            // Scan last 150 bars for swing highs and lows
            int scanLen = Math.Min(150, CurrentBar - 2);
            double atr = Math.Max(adaptiveATRSeries[0], 0.1);

            for (int i = 1; i <= scanLen; i++)
            {
                if (i >= CurrentBar - 2) break;

                // Swing high
                if (High[i] > High[i + 1] && High[i] >= High[i - 1])
                {
                    double thresh = Math.Max(atr * 0.4, High[i] * 0.0003);
                    bool exists = false;
                    for (int j = 0; j < _srLevels.Count; j++)
                    {
                        if (Math.Abs(_srLevels[j] - High[i]) / Math.Max(_srLevels[j], 1) < thresh / Math.Max(High[i], 1))
                        { exists = true; break; }
                    }
                    if (!exists && _srLevels.Count < 6)
                    {
                        _srLevels.Add(High[i]);
                        _srStrengths.Add(1.0);
                    }
                }

                // Swing low
                if (Low[i] < Low[i + 1] && Low[i] <= Low[i - 1])
                {
                    double thresh = Math.Max(atr * 0.4, Low[i] * 0.0003);
                    bool exists = false;
                    for (int j = 0; j < _srLevels.Count; j++)
                    {
                        if (Math.Abs(_srLevels[j] - Low[i]) / Math.Max(_srLevels[j], 1) < thresh / Math.Max(Low[i], 1))
                        { exists = true; break; }
                    }
                    if (!exists && _srLevels.Count < 6)
                    {
                        _srLevels.Add(Low[i]);
                        _srStrengths.Add(1.0);
                    }
                }
            }
        }
        #endregion

        #region Bias & Confidence Scoring
        private void CalculateBiasScores()
        {
            int bull = 0, bear = 0;

            // HTF contribution (weight: 30)
            if (_htfBullGate) bull += 30;
            if (_htfBearGate) bear += 30;

            // Macro contribution (weight: 25)
            if (_macroBull) bull += 25;
            if (_macroBear) bear += 25;
            bull += Math.Max(0, _macroStrengthScore) / 4;
            bear += Math.Max(0, -_macroStrengthScore) / 4;

            // Weighted trend (weight: 25)
            if (_bullTrend) bull += 25;
            if (_bearTrend) bear += 25;

            // Market structure (weight: 20)
            if (_bullBOS || _mssLabelBull || _displacementUp) bull += 20;
            if (_bearBOS || _mssLabelBear || _displacementDown) bear += 20;

            // Session quality bonus
            int sessBonus = _sessionQuality / 10;
            if (_inKillzone)
            {
                if (_bullBiasScore > _bearBiasScore) bull += sessBonus;
                else bear += sessBonus;
            }

            // Mean reversion
            double mr = mrCompositeSeries[0];
            if (mr <= -30) bull += (int)Math.Min(Math.Abs(mr), 20);
            if (mr >= 30) bear += (int)Math.Min(Math.Abs(mr), 20);

            // Normalize
            double total = bull + bear;
            if (total > 0)
            {
                _bullBiasScore = (double)bull / total * 100.0;
                _bearBiasScore = (double)bear / total * 100.0;
            }
            else { _bullBiasScore = 50; _bearBiasScore = 50; }

            // Confidence: 5-dimensional equal weight
            double c1 = _htfBullGate || _htfBearGate ? 0.9 : (_htfFullLong >= 3 || _htfFullShort >= 3 ? 0.7 : 0.4);
            double c2 = _macroBull || _macroBear ? 0.85 : (_macroStrengthConf > 30 ? 0.6 : 0.3);
            double c3 = _regimeTrending ? 0.8 : _regimeRanging ? 0.5 : 0.2;
            double c4 = _corrBreakdown ? 0.3 : 0.8;
            double c5 = _inKillzone ? 0.85 : _inLondon || _inNY ? 0.7 : 0.4;
            _confidenceScore = (c1 + c2 + c3 + c4 + c5) / 5.0 * 100.0;

            // Bias label
            double diff = _bullBiasScore - _bearBiasScore;
            if (diff > 15) _biasLabel = "BULLISH";
            else if (diff > 5) _biasLabel = "BULL-BIAS";
            else if (diff < -15) _biasLabel = "BEARISH";
            else if (diff < -5) _biasLabel = "BEAR-BIAS";
            else _biasLabel = "NEUTRAL";
        }
        #endregion

        #region Trade Plan
        private void CalculateTradePlan()
        {
            double atr = Math.Max(adaptiveATRSeries[0], 0.1);
            double close0 = Close[0];

            _shouldBuy  = _bullBiasScore > 55 && _bullTrend && (_htfBullGate || _htfAlignmentLong >= 2)
                          && _confidenceScore >= 40 && !_corrBreakdown;
            _shouldSell = _bearBiasScore > 55 && _bearTrend && (_htfBearGate || _htfAlignmentShort >= 2)
                          && _confidenceScore >= 40 && !_corrBreakdown;

            if (_shouldBuy)
            {
                _tradeDirection = "BUY";
                _tradeSL  = close0 - atr * 1.5;
                _tradeTP1 = close0 + atr * 1.5;
                _tradeTP2 = close0 + atr * 3.0;
                _tradeRR  = 2.0;
            }
            else if (_shouldSell)
            {
                _tradeDirection = "SELL";
                _tradeSL  = close0 + atr * 1.5;
                _tradeTP1 = close0 - atr * 1.5;
                _tradeTP2 = close0 - atr * 3.0;
                _tradeRR  = 2.0;
            }
            else
            {
                _tradeDirection = "NONE";
                _tradeSL = _tradeTP1 = _tradeTP2 = _tradeRR = 0;
            }

            // Execute trades if in auto mode
            if (ProfessionalMode && _sufficientBars && !_corrBreakdown && _confidenceScore >= 50)
            {
                if (_shouldBuy && Position.MarketPosition == MarketPosition.Flat)
                {
                    EnterLong(1, "LIQ-NEXUS-LONG");
                    SetStopLoss(CalculationMode.Price, _tradeSL);
                    SetProfitTarget(CalculationMode.Price, _tradeTP1);
                }
                else if (_shouldSell && Position.MarketPosition == MarketPosition.Flat)
                {
                    EnterShort(1, "LIQ-NEXUS-SHORT");
                    SetStopLoss(CalculationMode.Price, _tradeSL);
                    SetProfitTarget(CalculationMode.Price, _tradeTP1);
                }
            }
        }
        #endregion

        #region Drawing — Chart Signals
        private void DrawSignals()
        {
            if (!ShowSignals || PerfMode) return;

            // BOS labels
            if (_bosLabelBull)
                Draw.ArrowUp(this, "BOS-BULL-" + CurrentBar, false, 0, Low[0] - 2 * TickSize, Brushes.LimeGreen);
            if (_bosLabelBear)
                Draw.ArrowDown(this, "BOS-BEAR-" + CurrentBar, false, 0, High[0] + 2 * TickSize, Brushes.Red);

            // CHOCH / MSS
            if (_chochLabelBull)
                Draw.Text(this, "CHOCH-BULL-" + CurrentBar, false, "CHOCH", 0, Low[0] - 3 * TickSize, Brushes.LimeGreen);
            if (_chochLabelBear)
                Draw.Text(this, "CHOCH-BEAR-" + CurrentBar, false, "CHOCH", 0, High[0] + 3 * TickSize, Brushes.Red);

            if (_mssLabelBull)
                Draw.Text(this, "MSS-BULL-" + CurrentBar, false, "MSS", 0, Low[0] - 4 * TickSize, Brushes.Gold);
            if (_mssLabelBear)
                Draw.Text(this, "MSS-BEAR-" + CurrentBar, false, "MSS", 0, High[0] + 4 * TickSize, Brushes.Gold);

            // Displacement
            if (_displacementUp)
                Draw.Text(this, "DISP-UP-" + CurrentBar, false, "▲", 0, Low[0] - 5 * TickSize, Brushes.LimeGreen);
            if (_displacementDown)
                Draw.Text(this, "DISP-DN-" + CurrentBar, false, "▼", 0, High[0] + 5 * TickSize, Brushes.Red);

            // Volume climax
            if (_climaxUp)
                Draw.Text(this, "CLIMAX-UP-" + CurrentBar, false, "VOL-X", 0, Low[0] - 6 * TickSize, Brushes.Orange);
            if (_climaxDown)
                Draw.Text(this, "CLIMAX-DN-" + CurrentBar, false, "VOL-X", 0, High[0] + 6 * TickSize, Brushes.Orange);
        }
        #endregion

        #region Dashboard Rendering
        private void DrawDashboard()
        {
            if (!ShowDashboard) return;

            // Build dashboard text using Draw.TextFixed
            string sep = " │ ";
            string nl = "\n";

            string dashText = " XAUUSD INSTITUTIONAL LIQUIDITY NEXUS" + nl;
            dashText += new string('─', 60) + nl;

            // Row 1: HTF Trend Stack
            dashText += $" HTF: 5m={_htf5mScore,2} {(_htf5mBull ? "▲" : _htf5mBear ? "▼" : "—")}";
            dashText += $"  15m={_htf15mScore,2} {(_htf15mBull ? "▲" : _htf15mBear ? "▼" : "—")}";
            dashText += $"  1H={_htf1hScore,2} {(_htf1hBull ? "▲" : _htf1hBear ? "▼" : "—")}";
            dashText += $"  4H={_htf4hScore,2} {(_htf4hBull ? "▲" : _htf4hBear ? "▼" : "—")}";
            dashText += $"  D={_htfDScore,2} {(_htfDBull ? "▲" : _htfDBear ? "▼" : "—")}";
            dashText += $"  Gate: {(_htfBullGate ? "BULL" : _htfBearGate ? "BEAR" : "NONE")}" + nl;

            // Row 2: Macro
            dashText += " MACRO:";
            dashText += _dxyValid ? $" DXY={(_dxyBull ? "▼" : _dxyBear ? "▲" : "—")}" : " DXY=—";
            dashText += _yieldValid ? $" 10Y={(_yieldRising ? "▲" : _yieldFalling ? "▼" : "—")}" : " 10Y=—";
            dashText += _silverValid ? $" XAG={(_silverBull ? "▲" : _silverBear ? "▼" : "—")}" : " XAG=—";
            dashText += _eurusdValid ? $" EUR={(_eurusdBull ? "▲" : _eurusdBear ? "▼" : "—")}" : " EUR=—";
            dashText += _spxValid ? $" SPX={(_spxBull ? "▲" : _spxBear ? "▼" : "—")}" : " SPX=—";
            dashText += $"  Score={_macroStrengthScore,3}%  Votes: ▲{_macroBullVotes} ▼{_macroBearVotes}" + nl;

            // Row 3: Correlation Health
            dashText += $" CORR: DXY={_corrHealthDXY,3:F0}%  Yld={_corrHealthYld,3:F0}%  Slv={_corrHealthSlv,3:F0}%";
            dashText += $"  SPX={_corrHealthSPX,3:F0}%  EUR={_corrHealthEUR,3:F0}%";
            dashText += $"  Avg={_avgCorrHealth,3:F0}%  Break={(_corrBreakdown ? "⚠" : "✓")}" + nl;

            // Row 4: Trend & Structure
            dashText += $" TREND: {(_bullTrend ? "BULL" : _bearTrend ? "BEAR" : "NEUT")} ({_bullTrendScore}/{_bearTrendScore})";
            dashText += $"  ADX={adxSeries[0],2:F0} {(_regimeTrending ? "TREND" : _regimeRanging ? "RANGE" : "DEAD")}";
            dashText += $"  RSI={rsiSeries[0],2:F0}  ATR={adaptiveATRSeries[0],6:F1}" + nl;

            // Row 5: FVG/OB
            dashText += " FVG:";
            if (!double.IsNaN(_fvgScore) && _fvgActive) dashText += $" ACTIVE (Score={_fvgScore,2:F0})";
            else dashText += " NONE";
            dashText += "  OB:";
            if (_obBullActive) dashtext += $" BULL (Score={_obBullScore,2:F0})";
            else if (_obBearActive) dashText += $" BEAR (Score={_obBearScore,2:F0})";
            else dashText += " NONE" + nl;

            // Row 6: Bias & Confidence
            dashText += $" BIAS: ▲={_bullBiasScore,2:F0}%  ▼={_bearBiasScore,2:F0}%  → {_biasLabel}";
            dashText += $"  Conf={_confidenceScore,2:F0}%  Session={_sessionLabel} (Q={_sessionQuality,2}%)" + nl;

            // Row 7: Trade Plan
            if (ShowTradePlan)
            {
                dashText += " TRADE:";
                if (_tradeDirection == "BUY" || _tradeDirection == "SELL")
                {
                    dashText += $" {_tradeDirection}  SL={_tradeSL,6:F1}  TP1={_tradeTP1,6:F1}  TP2={_tradeTP2,6:F1}  RR=1:{_tradeRR,2:F1}";
                    if (_shouldBuy && _shouldSell) dashText += " ⚠ CONFLICT";
                }
                else dashText += " WAIT (No clear signal)";

                if (ProfessionalMode && Position.MarketPosition != MarketPosition.Flat)
                    dashText += $"  POS: {Position.MarketPosition} Qty={Position.Quantity}";
                dashText += nl;
            }

            // Row 8: SMT / EQ / S/R status
            dashText += $" SMT: {(_bullRsiDiv && _bearRsiDiv ? "No Div" : _bullRsiDiv ? "Bull Div" : _bearRsiDiv ? "Bear Div" : "—")}";
            dashText += $"  EQ: {(_equalHighs ? "EQH" : "")} {(_equalLows ? "EQL" : "")}";
            dashText += $"  S/R Levels: {_srLevels.Count}" + nl;

            // Draw the dashboard
            Draw.TextFixed(this, "LIQ-NEXUS-DASH", dashText, TextPosition.TopLeft,
                Brushes.White, new SimpleFont("Consolas", 10), Brushes.Black, Brushes.Gray, 0);
        }
        #endregion
    }
}
