//=============================================================================
// XAUUSD Quantum 3.0 — cTrader Automate Port (C#)
// Ported from Pine Script v6 (TradingView)
//
// DISCLAIMER: "Score" values are ordinal evidence-weighted composites,
// not statistically validated probabilities. Use for directional bias only,
// not for position sizing. EV/Kelly metrics are heuristic — they use ordinal
// scores as if they were probabilities, which is methodologically invalid.
// Historical analog is typically underpowered (3-10 matches, SE ~15-30%).
// Past outcomes do not guarantee future results.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class XAUUSDQuantum3 : Indicator
    {
        //=============================================================================
        // REGION: PARAMETERS
        //=============================================================================
        
        //--- MODULE TOGGLES ---
        [Parameter("Dashboard", DefaultValue = true)]
        public bool ShowDashboard { get; set; }
        
        [Parameter("Trade Plan", DefaultValue = true)]
        public bool ShowTradePlan { get; set; }
        
        [Parameter("Show VWAP", DefaultValue = true)]
        public bool ShowVWAP { get; set; }
        
        [Parameter("Show S/R", DefaultValue = true)]
        public bool ShowSR { get; set; }
        
        [Parameter("Show Sessions", DefaultValue = true)]
        public bool ShowSessions { get; set; }
        
        [Parameter("Show Signals", DefaultValue = true)]
        public bool ShowSignals { get; set; }
        
        [Parameter("Show FVG", DefaultValue = true)]
        public bool ShowFVG { get; set; }
        
        [Parameter("Show OB", DefaultValue = true)]
        public bool ShowOB { get; set; }
        
        [Parameter("Show SMT", DefaultValue = true)]
        public bool ShowSMT { get; set; }
        
        [Parameter("Show Asian Sweeps", DefaultValue = true)]
        public bool ShowAsianSweeps { get; set; }
        
        [Parameter("Show Climax", DefaultValue = true)]
        public bool ShowClimax { get; set; }
        
        [Parameter("Show Adaptive ATR", DefaultValue = true)]
        public bool ShowAdaptiveATR { get; set; }
        
        [Parameter("Show Displacement", DefaultValue = true)]
        public bool ShowDisplacement { get; set; }
        
        [Parameter("Show Manipulation", DefaultValue = true)]
        public bool ShowManipulation { get; set; }
        
        [Parameter("Show Forecast Cone", DefaultValue = true)]
        public bool ShowForecastCone { get; set; }
        
        [Parameter("Show Premium Disc", DefaultValue = true)]
        public bool ShowPremiumDisc { get; set; }
        
        [Parameter("Show Stats Engine", DefaultValue = true)]
        public bool ShowStatsEngine { get; set; }
        
        [Parameter("Show Corr Health", DefaultValue = true)]
        public bool ShowCorrHealth { get; set; }
        
        [Parameter("Show MSS", DefaultValue = true)]
        public bool ShowMSS { get; set; }
        
        [Parameter("Perf Mode", DefaultValue = false)]
        public bool PerfMode { get; set; }
        
        [Parameter("Show EMAs", DefaultValue = true)]
        public bool ShowEMAs { get; set; }
        
        [Parameter("Show ADX", DefaultValue = true)]
        public bool ShowADX { get; set; }
        
        [Parameter("Show Macro", DefaultValue = true)]
        public bool ShowMacro { get; set; }
        
        [Parameter("Show PDH", DefaultValue = true)]
        public bool ShowPDH { get; set; }
        
        [Parameter("Show PDL", DefaultValue = true)]
        public bool ShowPDL { get; set; }
        
        [Parameter("Show PWH", DefaultValue = true)]
        public bool ShowPWH { get; set; }
        
        [Parameter("Show PWL", DefaultValue = true)]
        public bool ShowPWL { get; set; }
        
        [Parameter("Show Liq Pools", DefaultValue = true)]
        public bool ShowLiqPools { get; set; }
        
        [Parameter("Show Macro Panel", DefaultValue = true)]
        public bool ShowMacroPanel { get; set; }
        
        //--- CORE INPUTS ---
        [Parameter("EMA 20", DefaultValue = 20)]
        public int Ema20Len { get; set; }
        
        [Parameter("EMA 100", DefaultValue = 100)]
        public int Ema100Len { get; set; }
        
        [Parameter("EMA 200", DefaultValue = 200)]
        public int Ema200Len { get; set; }
        
        [Parameter("ATR Length", DefaultValue = 14)]
        public int AtrLen { get; set; }
        
        [Parameter("ADX Length", DefaultValue = 14)]
        public int AdxLen { get; set; }
        
        [Parameter("ADX Threshold", DefaultValue = 25)]
        public int AdxThreshold { get; set; }
        
        [Parameter("RSI Length", DefaultValue = 14)]
        public int RsiLength { get; set; }
        
        [Parameter("Internal Pivot Len", DefaultValue = 2)]
        public int InternalPivotLen { get; set; }
        
        [Parameter("Swing Pivot Len", DefaultValue = 5)]
        public int SwingPivotLen { get; set; }
        
        [Parameter("Recent Bars Len", DefaultValue = 120)]
        public int RecentBarsLen { get; set; }
        
        [Parameter("Trend Threshold", DefaultValue = 60)]
        public int TrendThreshold { get; set; }
        
        [Parameter("Vol Climax %", DefaultValue = 95)]
        public int VolClimaxPerc { get; set; }
        
        [Parameter("EQ ATR Mult", DefaultValue = 0.25)]
        public double EqAtrMult { get; set; }
        
        [Parameter("Vol Lookback", DefaultValue = 50)]
        public int VolLookback { get; set; }
        
        [Parameter("FVG Max Bars", DefaultValue = 30)]
        public int FvgMaxBars { get; set; }
        
        [Parameter("OB Max Bars", DefaultValue = 20)]
        public int ObMaxBars { get; set; }
        
        [Parameter("Liq Prox Mult", DefaultValue = 1.5)]
        public double LiqProxMult { get; set; }
        
        //--- V11 ---
        [Parameter("Disp Mult", DefaultValue = 2.5)]
        public double DispMult { get; set; }
        
        [Parameter("Manip Threshold", DefaultValue = 0.5)]
        public double ManipThreshold { get; set; }
        
        //--- V17 ---
        [Parameter("DXY ROC Len", DefaultValue = 10)]
        public int DxyRocLen { get; set; }
        
        [Parameter("Use RSI Div Filter", DefaultValue = true)]
        public bool UseRsiDivFilter { get; set; }
        
        [Parameter("Corr Health Bars", DefaultValue = 10)]
        public int CorrHealthBars { get; set; }
        
        [Parameter("ZN ROC Len", DefaultValue = 10)]
        public int ZnRocLen { get; set; }
        
        [Parameter("Forecast Bars", DefaultValue = 10)]
        public int ForecastBars { get; set; }
        
        [Parameter("Forecast Z", DefaultValue = 2.0)]
        public double ForecastZ { get; set; }
        
        [Parameter("Invert Yield", DefaultValue = false)]
        public bool InvertYield { get; set; }
        
        //--- V19 ---
        [Parameter("Corr Short Len", DefaultValue = 30)]
        public int CorrShortLen { get; set; }
        
        [Parameter("Corr Med Len", DefaultValue = 60)]
        public int CorrMedLen { get; set; }
        
        [Parameter("Corr Long Len", DefaultValue = 120)]
        public int CorrLongLen { get; set; }
        
        //--- STATISTICAL ---
        [Parameter("Enable Analog Engine", DefaultValue = true)]
        public bool EnableAnalogEngine { get; set; }
        
        [Parameter("Enable Stats Engine", DefaultValue = true)]
        public bool EnableStatsEngine { get; set; }
        
        [Parameter("Use Throttle", DefaultValue = true)]
        public bool UseThrottle { get; set; }
        
        [Parameter("Outcome N", DefaultValue = 10)]
        public int OutcomeN { get; set; }
        
        [Parameter("Hist Max", DefaultValue = 4500)]
        public int HistMax { get; set; }
        
        [Parameter("Softmax Temperature", DefaultValue = 1.0)]
        public double SoftmaxTemperature { get; set; }
        
        [Parameter("Min Prob Threshold", DefaultValue = 55.0)]
        public double MinProbThreshold { get; set; }
        
        [Parameter("Min Signal Quality", DefaultValue = 50.0)]
        public double MinSignalQuality { get; set; }
        
        //=============================================================================
        // REGION: CONSTANTS
        //=============================================================================
        
        private const int HIST_MAX = 4500;
        private const int OUTCOME_N = 10;
        private const int SIM_THRESHOLD = 55;
        private const int VP_BUCKETS = 40;
        private const int SR_SCAN_LEN = 150;
        private const int SR_CLUSTER_WINDOW = 5;
        private const int SR_REPEAT_MIN = 3;
        private const double SR_BUFFER_MULTIPLIER = 0.3;
        private const int DIV_LEN = 50;
        private const int VP_LOOKBACK = 100;
        private const int LIQ_SCAN = 50;
        private const int MAX_SCAN_BARS = 200;
        private const double SQ_MAX = 100.0;
        
        //=============================================================================
        // REGION: INDICATOR OUTPUTS
        //=============================================================================
        
        [Output("Bull Prob", LineColor = "Green")]
        public IndicatorDataSeries BullProbOutput { get; set; }
        
        [Output("Bear Prob", LineColor = "Red")]
        public IndicatorDataSeries BearProbOutput { get; set; }
        
        //=============================================================================
        // REGION: STATE VARIABLES
        //=============================================================================
        
        private double _goldRet;
        private double _ema20, _ema100, _ema200;
        private double _atr, _adxVal, _diPlus, _diMinus;
        private double _vwapVal, _rsiVal;
        private double _volSma20;
        private bool _highVolume;
        private bool _recentBars;
        
        private double _atrTrending, _atrRanging;
        private bool _atrInTrendingRegime;
        private double _rawAdaptiveATR, _adaptiveATR;
        private double _atrPercentile;
        
        private double _retMean20, _retStd20, _retZScore;
        private double _retSkew, _retKurt, _retCFAdj, _retCFZ;
        private double _distVwapATR, _distEma20ATR, _distEma100ATR, _distEma200ATR;
        private double _mrComposite, _mrBullScore, _mrBearScore;
        private string _mrRegime;
        private double _bbMiddle, _bbUpper, _bbLower;
        private bool _regimeTrending, _regimeRanging, _regimeDead;
        
        private int _hourUTC, _dom, _mon, _yr, _dow;
        private bool _usIsDST, _euIsDST;
        private int _londonOpen, _londonClose, _nyOpen, _nyClose;
        private bool _inAsian, _inLondon, _inNY, _inLondonKZ, _inLondonFixKZ, _inNYKZ, _inLondonCloseKZ, _inKillzone;
        private int _sessionQuality;
        private string _sessionLabel;
        private int _usDstStartDom = 8, _usDstEndDom = 1;
        private int _euDstStartDom = 25, _euDstEndDom = 25;
        private int _lastUsDstYr, _lastEuDstYr;
        
        private int _bullTrendScore, _bearTrendScore;
        private bool _bullTrend, _bearTrend;
        
        private double _bosResistanceSnap, _bosSupportSnap;
        private double _swingBosResSnap, _swingBosSupSnap;
        private bool _bullBOS, _bearBOS;
        private bool _bosLabelBull, _bosLabelBear;
        private bool _swingBosLabelBull, _swingBosLabelBear;
        
        private bool _chochBull, _chochBear, _mssBull, _mssBear, _mss;
        
        private bool _displacementBull, _displacementBear;
        private double _displacementScore;
        
        private bool _bullRsiDiv, _bearRsiDiv;
        
        private bool _equalHighs, _equalLows;
        
        private double _fvgBullUp, _fvgBullDown, _fvgBearUp, _fvgBearDown, _fvg;
        
        private bool _obBull, _obBear;
        private double _obBullLow, _obBullHigh, _obBearLow, _obBearHigh;
        private int _lastObBullBar = -10, _lastObBearBar = -10;
        
        private List<double> _srLevels;
        private List<double> _srStrengths;
        private List<double> _srResistances;
        private List<double> _srSupports;
        private double _srNearestRes = double.NaN, _srNearestSup = double.NaN;
        private double _srNearestRes2 = double.NaN, _srNearestSup2 = double.NaN;
        
        private double _vpvrPOC, _vpvrVAH, _vpvrVAL;
        private bool _vpvrBreakUp, _vpvrBreakDown;
        
        private double _liquidityAbove = double.NaN, _liquidityBelow = double.NaN;
        private bool _liquiditySweptAbove, _liquiditySweptBelow;
        
        private bool _climaxVolReversal, _climaxRange;
        
        private double _regimeDominant;
        private string _regimeBias = "NEUTRAL";
        private double _regimeConviction;
        
        private double _bullProb = 50.0, _bearProb = 50.0;
        private double _probDominant, _probSpread;
        
        private bool _analogActive;
        private int _analogBestIdx;
        private double _analogSimScore, _analogPrevOC, _analogPRevROC, _analogConf;
        private List<double> _analogOutcomes;
        private List<double> _analogFrequencies;
        
        private int _scenarioBullCount, _scenarioBearCount;
        private string _scenarioBias = "NEUTRAL";
        private int _scenarioConviction;
        
        private double _signalQuality;
        private string _entryGrade = "F";
        
        private int _lastStatsBar = -1, _lastStatsRunBar = -1;
        private int _statMatchCount, _statWinCount, _statLossCount;
        private double _statTotalRet, _statAvgWin, _statAvgLoss;
        private double _statWinRate = 50.0, _statAvgRet, _probAdjusted = 50.0;
        
        private double[] _goldRetArray;
        private double[] _atrArray;
        private double[] _volArray;
        private double[] _rangeArray;
        
        private double _activeResistance = double.NaN, _activeSupport = double.NaN;
        private double _lastBrokenRes = double.NaN, _lastBrokenSup = double.NaN;
        private double _swingActiveResistance = double.NaN, _swingActiveSupport = double.NaN;
        private double _swingLastBrokenRes = double.NaN, _swingLastBrokenSup = double.NaN;
        
        private bool _inOffHours;
        
        //--- Signal Quality & Trade Plan ---
        private int _lastSignalBar = -1;
        private string _lastSignalGrade = "";
        private bool _shouldBuy, _shouldSell;
        private bool _tpIsLong;
        private double _tpEntry, _tpSL, _tp1, _tp2, _tpRR1;
        private string _tpDirStr, _tpEntryStr, _tpSLStr, _tp1Str, _tp2Str, _tpRRStr;
        
        //=============================================================================
        // REGION: LIFECYCLE
        //=============================================================================
        
        protected override void Initialize()
        {
            _srLevels = new List<double>();
            _srStrengths = new List<double>();
            _srResistances = new List<double>();
            _srSupports = new List<double>();
            _analogOutcomes = new List<double>();
            _analogFrequencies = new List<double>();
            
            _goldRetArray = new double[HIST_MAX];
            _atrArray = new double[HIST_MAX];
            _volArray = new double[HIST_MAX];
            _rangeArray = new double[HIST_MAX];
            for (int i = 0; i < HIST_MAX; i++)
            {
                _goldRetArray[i] = 0.0;
                _atrArray[i] = 0.0;
                _volArray[i] = 0.0;
                _rangeArray[i] = 0.0;
            }
        }
        
        public override void Calculate(int index)
        {
            if (index < 1) return;
            
            UpdateRollingArrays(index);
            UpdateCoreTA(index);
            UpdateAdaptiveATR(index);
            UpdateMeanReversion(index);
            UpdateSessions(index);
            UpdateTrendEngine(index);
            UpdateStructureEngine(index);
            UpdateBOSDetection(index);
            UpdateCHOCHMSS();
            UpdateDisplacement(index);
            UpdateRSIDivergence(index);
            UpdateEQHEQL(index);
            UpdateFVG(index);
            UpdateOrderBlocks(index);
            UpdateSR(index);
            UpdateVPVR(index);
            UpdateLiquidity(index);
            UpdateClimax(index);
            UpdateRegimeEngine();
            UpdateProbabilityEngine();
            UpdateHistoricalAnalog(index);
            UpdateScenario();
            UpdateSignalQuality(index);
            RunStatsEngines(index);
            UpdateTradePlan(index);
            
            // Outputs
            BullProbOutput[index] = _bullProb;
            BearProbOutput[index] = _bearProb;
            
            // Drawing (last bar only)
            if (index == Bars.Count - 1)
            {
                UpdateDashboard();
                UpdateChartDrawing();
            }
        }
        
        //=============================================================================
        // REGION: DATA HELPERS
        //=============================================================================
        
        private double Close(int index) => Bars.ClosePrices[index];
        private double Open(int index) => Bars.OpenPrices[index];
        private double High(int index) => Bars.HighPrices[index];
        private double Low(int index) => Bars.LowPrices[index];
        private long Volume(int index) => Bars.TickVolumes[index];
        private DateTime Time(int index) => Bars.OpenTimes[index];
        
        private double SMA(DataSeries source, int period, int i)
        {
            if (i < period - 1) return double.NaN;
            double sum = 0;
            for (int j = i - period + 1; j <= i; j++)
                sum += source[j];
            return sum / period;
        }
        
        private double EMA(DataSeries source, int period, int i)
        {
            if (i < period - 1) return double.NaN;
            double alpha = 2.0 / (period + 1);
            double ema = source[i - period + 1];
            for (int j = i - period + 2; j <= i; j++)
                ema = source[j] * alpha + ema * (1.0 - alpha);
            return ema;
        }
        
        private double ATR(int period, int i)
        {
            if (i < period) return double.NaN;
            double sum = 0;
            for (int j = i - period + 1; j <= i; j++)
            {
                double tr = Math.Max(High(j) - Low(j), Math.Max(Math.Abs(High(j) - Close(j - 1)), Math.Abs(Low(j) - Close(j - 1))));
                sum += tr;
            }
            return sum / period;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Rolling Arrays
        //=============================================================================
        
        private void UpdateRollingArrays(int index)
        {
            if (index >= 1 && index < HIST_MAX)
            {
                _goldRetArray[index] = _goldRet;
                _atrArray[index] = _adaptiveATR;
                _volArray[index] = Volume(index);
                _rangeArray[index] = _adaptiveATR > 0 ? (High(index) - Low(index)) / _adaptiveATR : 1.0;
            }
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Core TA
        //=============================================================================
        
        private void UpdateCoreTA(int index)
        {
            _goldRet = Close(index) / Close(index - 1) - 1.0;
            _ema20 = EMA(Bars.ClosePrices, Ema20Len, index);
            _ema100 = EMA(Bars.ClosePrices, Ema100Len, index);
            _ema200 = EMA(Bars.ClosePrices, Ema200Len, index);
            _atr = ATR(AtrLen, index);
            _rsiVal = RSI(index);
            _vwapVal = VWAP(index);
            
            _volSma20 = SMA(Bars.TickVolumes, 20, index);
            _highVolume = Volume(index) > _volSma20;
            _recentBars = index > Bars.Count - 1 - RecentBarsLen;
        }
        
        private double RSI(int index)
        {
            int period = RsiLength;
            if (index < period + 1) return 50.0;
            double avgGain = 0, avgLoss = 0;
            for (int i = index - period + 1; i <= index; i++)
            {
                double ch = Close(i) - Close(i - 1);
                if (ch >= 0) avgGain += ch;
                else avgLoss -= ch;
            }
            avgGain /= period;
            avgLoss /= period;
            if (avgLoss == 0) return 100.0;
            double rs = avgGain / avgLoss;
            return 100.0 - 100.0 / (1.0 + rs);
        }
        
        private double VWAP(int index)
        {
            if (index < 1) return Close(index);
            double cumPV = 0, cumVol = 0;
            for (int i = 0; i <= index; i++)
            {
                double typ = (High(i) + Low(i) + Close(i)) / 3.0;
                cumPV += typ * Volume(i);
                cumVol += Volume(i);
            }
            return cumVol > 0 ? cumPV / cumVol : Close(index);
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Adaptive ATR Regime
        //=============================================================================
        
        private void UpdateAdaptiveATR(int index)
        {
            _atrTrending = ATR(7, index);
            _atrRanging = ATR(21, index);
            
            if (index >= 2)
                _atrInTrendingRegime = _atrInTrendingRegime
                    ? _adxVal > (AdxThreshold - 3)
                    : _adxVal > (AdxThreshold + 3);
            
            _rawAdaptiveATR = ShowAdaptiveATR
                ? (_atrInTrendingRegime ? _atrTrending : _atrRanging)
                : _atr;
            _adaptiveATR = Math.Max(Nz(_rawAdaptiveATR), Close(index) * 0.0001);
            _atrPercentile = PercentRank(_adaptiveATR, 50, index);
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Mean Reversion
        //=============================================================================
        
        private void UpdateMeanReversion(int index)
        {
            _retMean20 = SMA(_goldRet, 20, index);
            _retStd20 = StdDev(_goldRet, 20, index);
            _retZScore = _retStd20 > 0.0 ? (_goldRet - _retMean20) / _retStd20 : 0.0;
            
            double n = 20.0;
            double mu = _retMean20;
            double ret = _goldRet;
            double m2 = SMA(ret * ret, 20, index) * n;
            double m3 = SMA(ret * ret * ret, 20, index) * n;
            double m4 = SMA(ret * ret * ret * ret, 20, index) * n;
            double v = Math.Max(m2 / n - mu * mu, 0.000001);
            double sd = Math.Sqrt(v);
            _retSkew = sd > 0 ? (m3 / n - 3.0 * mu * m2 / n + 2.0 * mu * mu * mu) / (sd * sd * sd) : 0.0;
            _retKurt = v > 0 ? (m4 / n - 4.0 * mu * m3 / n + 6.0 * mu * mu * m2 / n - 3.0 * mu * mu * mu * mu) / (v * v) - 3.0 : 0.0;
            
            _retCFAdj = 1.0 + (_retSkew / 6.0) * (ForecastZ * ForecastZ - 1.0)
                + (_retKurt / 24.0) * (ForecastZ * ForecastZ * ForecastZ - 3.0 * ForecastZ)
                - (_retSkew * _retSkew / 36.0) * (2.0 * ForecastZ * ForecastZ * ForecastZ - 5.0 * ForecastZ);
            _retCFZ = Math.Max(ForecastZ * 0.5, ForecastZ * _retCFAdj);
            
            _distVwapATR = _adaptiveATR > 0 ? (Close(index) - _vwapVal) / _adaptiveATR : 0.0;
            _distEma20ATR = _adaptiveATR > 0 ? (Close(index) - _ema20) / _adaptiveATR : 0.0;
            _distEma100ATR = _adaptiveATR > 0 ? (Close(index) - _ema100) / _adaptiveATR : 0.0;
            _distEma200ATR = _adaptiveATR > 0 ? (Close(index) - _ema200) / _adaptiveATR : 0.0;
            
            _mrComposite = Math.Min(Math.Max(
                _distVwapATR * 30.0 + _distEma20ATR * 25.0 + _distEma100ATR * 25.0 + _distEma200ATR * 20.0 + _retZScore * 15.0,
                -100), 100);
            _mrRegime = Math.Abs(_mrComposite) >= 60 ? "Overextended" : Math.Abs(_mrComposite) >= 30 ? "Extended" : "Normal";
            _mrBullScore = _mrComposite <= -30 ? Math.Min(Math.Abs(_mrComposite), 100) : 10.0;
            _mrBearScore = _mrComposite >= 30 ? Math.Min(Math.Abs(_mrComposite), 100) : 10.0;
            
            var bb = Bollinger(index);
            _bbMiddle = bb.Item1; _bbUpper = bb.Item2; _bbLower = bb.Item3;
            
            _regimeTrending = _adxVal > AdxThreshold;
            _regimeRanging = _adxVal <= AdxThreshold && _adxVal > 15;
            _regimeDead = _adxVal <= 15;
        }
        
        private Tuple<double, double, double> Bollinger(int index)
        {
            int period = 20;
            double mult = 2.0;
            double sma = SMA(Bars.ClosePrices, period, index);
            double sumSq = 0;
            for (int i = index - period + 1; i <= index; i++)
                sumSq += Math.Pow(Close(i) - sma, 2);
            double std = Math.Sqrt(sumSq / period);
            return Tuple.Create(sma, sma + mult * std, sma - mult * std);
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Sessions (DST-aware)
        //=============================================================================
        
        private void UpdateSessions(int index)
        {
            DateTime utc = Time(index).ToUniversalTime();
            _hourUTC = utc.Hour;
            _dom = utc.Day;
            _mon = utc.Month;
            _yr = utc.Year;
            _dow = (int)utc.DayOfWeek + 1;
            
            int daysBackToSun = (_dow - 1 + 7) % 7;
            int thisSunDom = _dom - daysBackToSun;
            int firstSunDom = thisSunDom - 7 * (int)((thisSunDom - 1) / 7);
            if (firstSunDom < 1) firstSunDom += 7;
            
            if (_yr != _lastUsDstYr)
            {
                _usDstStartDom = 8; _usDstEndDom = 1; _lastUsDstYr = _yr;
            }
            if (_mon == 3) _usDstStartDom = firstSunDom + 7;
            if (_mon == 11) _usDstEndDom = firstSunDom;
            _usIsDST = (_mon > 3 && _mon < 11) ||
                       (_mon == 3 && _dom >= _usDstStartDom) ||
                       (_mon == 11 && _dom < _usDstEndDom);
            
            if (_yr != _lastEuDstYr)
            {
                _euDstStartDom = 25; _euDstEndDom = 25; _lastEuDstYr = _yr;
            }
            if (_mon == 3 && _dow == 1) _euDstStartDom = Math.Max(_euDstStartDom, _dom);
            if (_mon == 10 && _dow == 1) _euDstEndDom = Math.Max(_euDstEndDom, _dom);
            _euIsDST = (_mon > 3 && _mon < 10) ||
                       (_mon == 3 && _dom >= _euDstStartDom) ||
                       (_mon == 10 && _dom < _euDstEndDom);
            
            _londonOpen = 7;
            _londonClose = _euIsDST ? 16 : 17;
            _nyOpen = _usIsDST ? 13 : 14;
            _nyClose = _usIsDST ? 21 : 22;
            
            _inAsian = _hourUTC >= 0 && _hourUTC < _londonOpen;
            _inLondon = _hourUTC >= _londonOpen && _hourUTC < _londonClose;
            _inNY = _hourUTC >= _nyOpen && _hourUTC < _nyClose;
            _inLondonKZ = _hourUTC >= _londonOpen + 1 && _hourUTC < _londonOpen + 4;
            _inLondonFixKZ = _hourUTC >= _londonOpen + 3 && _hourUTC < _londonOpen + 4;
            _inNYKZ = _hourUTC >= _nyOpen && _hourUTC < _nyOpen + 3;
            _inLondonCloseKZ = _hourUTC >= _londonClose - 1 && _hourUTC < _londonClose;
            _inKillzone = _inLondonKZ || _inNYKZ || _inLondonCloseKZ || _inLondonFixKZ;
            
            double sq = 0;
            sq += _inKillzone ? 40 : _inLondon || _inNY ? 25 : _inAsian ? 10 : 0;
            sq += _inLondonFixKZ ? 20 : 0;
            sq += _inNYKZ ? 15 : 0;
            sq += (!_inAsian || _inLondonKZ) ? 10 : 0;
            sq += (_inLondon && !_inLondonKZ) ? 5 : 0;
            _sessionQuality = (int)Math.Round(sq / SQ_MAX * 100.0);
            
            if (_inKillzone)
                _sessionLabel = _inLondonFixKZ ? "LON-FIX" : _inLondonKZ ? "LON-KZ" : _inNYKZ ? "NY-KZ" : _inLondonCloseKZ ? "LON-CL" : "KZ";
            else if (_inLondon) _sessionLabel = "LONDON";
            else if (_inNY) _sessionLabel = "NY";
            else if (_inAsian) _sessionLabel = "ASIAN";
            else _sessionLabel = "OFF";
            _inOffHours = _sessionLabel == "OFF";
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Trend Engine
        //=============================================================================
        
        private void UpdateTrendEngine(int index)
        {
            _bullTrendScore = 0;
            _bullTrendScore += Close(index) > _ema200 ? 20 : 0;
            _bullTrendScore += _ema20 > _ema100 ? 20 : 0;
            _bullTrendScore += _diPlus > _diMinus ? 20 : 0;
            _bullTrendScore += _adxVal > AdxThreshold ? 20 : 0;
            _bullTrendScore += Close(index) > _ema100 ? 10 : 0;
            _bullTrendScore += _highVolume ? 10 : 0;
            
            _bearTrendScore = 0;
            _bearTrendScore += Close(index) < _ema200 ? 20 : 0;
            _bearTrendScore += _ema20 < _ema100 ? 20 : 0;
            _bearTrendScore += _diMinus > _diPlus ? 20 : 0;
            _bearTrendScore += _adxVal > AdxThreshold ? 20 : 0;
            _bearTrendScore += Close(index) < _ema100 ? 10 : 0;
            _bearTrendScore += _highVolume ? 10 : 0;
            
            _bullTrend = _bullTrendScore >= TrendThreshold;
            _bearTrend = _bearTrendScore >= TrendThreshold;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Structure Engine (Pivots, BOS)
        //=============================================================================
        
        private void UpdateStructureEngine(int index)
        {
            double pivotHigh = PivotHigh(InternalPivotLen, index);
            double pivotLow = PivotLow(InternalPivotLen, index);
            double swingPivotHigh = PivotHigh(SwingPivotLen, index);
            double swingPivotLow = PivotLow(SwingPivotLen, index);
            
            _bosResistanceSnap = _activeResistance;
            _bosSupportSnap = _activeSupport;
            
            if (!double.IsNaN(pivotHigh))
            {
                if (!double.IsNaN(_activeResistance)) _lastBrokenRes = _activeResistance;
                _activeResistance = pivotHigh;
            }
            if (!double.IsNaN(pivotLow))
            {
                if (!double.IsNaN(_activeSupport)) _lastBrokenSup = _activeSupport;
                _activeSupport = pivotLow;
            }
            
            double breakBuffer = _adaptiveATR * 0.15;
            double strongBody = _adaptiveATR * 0.30;
            bool strongVol = Volume(index) > _volSma20 * 1.20;
            
            if (!double.IsNaN(_activeResistance) && Close(index) > _activeResistance &&
                Close(index - 1) <= _activeResistance && Volume(index) > _volSma20)
            {
                _lastBrokenRes = _activeResistance;
                _activeResistance = double.NaN;
            }
            if (!double.IsNaN(_activeSupport) && Close(index) < _activeSupport &&
                Close(index - 1) >= _activeSupport && Volume(index) > _volSma20)
            {
                _lastBrokenSup = _activeSupport;
                _activeSupport = double.NaN;
            }
            
            _swingBosResSnap = _swingActiveResistance;
            _swingBosSupSnap = _swingActiveSupport;
            
            if (!double.IsNaN(swingPivotHigh))
            {
                if (!double.IsNaN(_swingActiveResistance)) _swingLastBrokenRes = _swingActiveResistance;
                _swingActiveResistance = swingPivotHigh;
            }
            if (!double.IsNaN(swingPivotLow))
            {
                if (!double.IsNaN(_swingActiveSupport)) _swingLastBrokenSup = _swingActiveSupport;
                _swingActiveSupport = swingPivotLow;
            }
            if (!double.IsNaN(_swingActiveResistance) && Close(index) > _swingActiveResistance &&
                Close(index - 1) <= _swingActiveResistance && Volume(index) > _volSma20)
            {
                _swingLastBrokenRes = _swingActiveResistance;
                _swingActiveResistance = double.NaN;
            }
            if (!double.IsNaN(_swingActiveSupport) && Close(index) < _swingActiveSupport &&
                Close(index - 1) >= _swingActiveSupport && Volume(index) > _volSma20)
            {
                _swingLastBrokenSup = _swingActiveSupport;
                _swingActiveSupport = double.NaN;
            }
        }
        
        private double PivotHigh(int leftRight, int index)
        {
            if (index < leftRight || index + leftRight >= Bars.Count) return double.NaN;
            double val = High(index);
            for (int i = index - leftRight; i <= index + leftRight; i++)
                if (i != index && High(i) > val) return double.NaN;
            return val;
        }
        
        private double PivotLow(int leftRight, int index)
        {
            if (index < leftRight || index + leftRight >= Bars.Count) return double.NaN;
            double val = Low(index);
            for (int i = index - leftRight; i <= index + leftRight; i++)
                if (i != index && Low(i) < val) return double.NaN;
            return val;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — BOS Detection
        //=============================================================================
        
        private void UpdateBOSDetection(int index)
        {
            double bodySize = Math.Abs(Close(index) - Open(index));
            double candleRange = High(index) - Low(index);
            double bodyPct = candleRange > 0 ? bodySize / candleRange * 100.0 : 0.0;
            double breakBuffer = _adaptiveATR * 0.15;
            double strongBody = _adaptiveATR * 0.30;
            bool strongVol = Volume(index) > _volSma20 * 1.20;
            
            bool intBullBOS = !double.IsNaN(_bosResistanceSnap) && Close(index) > _bosResistanceSnap &&
                Close(index - 1) <= _bosResistanceSnap && _bosResistanceSnap != _lastBrokenRes &&
                _adxVal > AdxThreshold && (Close(index) - _bosResistanceSnap) > breakBuffer &&
                bodySize > strongBody && bodyPct > 70 && strongVol;
            bool intBearBOS = !double.IsNaN(_bosSupportSnap) && Close(index) < _bosSupportSnap &&
                Close(index - 1) >= _bosSupportSnap && _bosSupportSnap != _lastBrokenSup &&
                _adxVal > AdxThreshold && (_bosSupportSnap - Close(index)) > breakBuffer &&
                bodySize > strongBody && bodyPct > 70 && strongVol;
            
            _bosLabelBull = !double.IsNaN(_bosResistanceSnap) && Close(index) > _bosResistanceSnap &&
                Close(index - 1) <= _bosResistanceSnap && _bosResistanceSnap != _lastBrokenRes;
            _bosLabelBear = !double.IsNaN(_bosSupportSnap) && Close(index) < _bosSupportSnap &&
                Close(index - 1) >= _bosSupportSnap && _bosSupportSnap != _lastBrokenSup;
            
            bool swBull = !double.IsNaN(_swingBosResSnap) && Close(index) > _swingBosResSnap &&
                Close(index - 1) <= _swingBosResSnap && _swingBosResSnap != _swingLastBrokenRes &&
                _adxVal > AdxThreshold && (Close(index) - _swingBosResSnap) > breakBuffer &&
                bodySize > strongBody && bodyPct > 70 && strongVol;
            bool swBear = !double.IsNaN(_swingBosSupSnap) && Close(index) < _swingBosSupSnap &&
                Close(index - 1) >= _swingBosSupSnap && _swingBosSupSnap != _swingLastBrokenSup &&
                _adxVal > AdxThreshold && (_swingBosSupSnap - Close(index)) > breakBuffer &&
                bodySize > strongBody && bodyPct > 70 && strongVol;
            
            _swingBosLabelBull = !double.IsNaN(_swingBosResSnap) && Close(index) > _swingBosResSnap &&
                Close(index - 1) <= _swingBosResSnap && _swingBosResSnap != _swingLastBrokenRes;
            _swingBosLabelBear = !double.IsNaN(_swingBosSupSnap) && Close(index) < _swingBosSupSnap &&
                Close(index - 1) >= _swingBosSupSnap && _swingBosSupSnap != _swingLastBrokenSup;
            
            _bullBOS = intBullBOS || swBull;
            _bearBOS = intBearBOS || swBear;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — CHOCH / MSS
        //=============================================================================
        
        private void UpdateCHOCHMSS()
        {
            _chochBull = !double.IsNaN(_swingLastBrokenRes);
            _chochBear = !double.IsNaN(_swingLastBrokenSup);
            _mssBull = _bosLabelBull && _chochBull;
            _mssBear = _bosLabelBear && _chochBear;
            _mss = _mssBull || _mssBear;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Displacement
        //=============================================================================
        
        private void UpdateDisplacement(int index)
        {
            double range = High(index) - Low(index);
            double body = Math.Abs(Close(index) - Open(index));
            double wickTop = High(index) - Math.Max(Open(index), Close(index));
            double wickBot = Math.Min(Open(index), Close(index)) - Low(index);
            double bodyPct = range > 0 ? body / range * 100.0 : 0.0;
            double wickTopPct = range > 0 ? wickTop / range * 100.0 : 0.0;
            double wickBotPct = range > 0 ? wickBot / range * 100.0 : 0.0;
            
            double displacement = range / Math.Max(_adaptiveATR, Close(index) * 0.0001);
            
            bool bullishBody = Close(index) > Open(index) && bodyPct > 30 && wickTopPct < 20;
            bool bearishBody = Close(index) < Open(index) && bodyPct > 30 && wickBotPct < 20;
            bool strongVol = Volume(index) > _volSma20 * 1.30;
            
            _displacementBull = bullishBody && strongVol && displacement >= 1.0 &&
                Close(index) >= _ema20 && _diPlus > _diMinus;
            _displacementBear = bearishBody && strongVol && displacement >= 1.0 &&
                Close(index) <= _ema20 && _diMinus > _diPlus;
            _displacementScore = displacement;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — RSI Divergence
        //=============================================================================
        
        private void UpdateRSIDivergence(int index)
        {
            if (index < DIV_LEN) return;
            
            double localLowPrice = double.MaxValue;
            double localLowRSI = double.MaxValue;
            int priceBarLow = 0;
            double prevLocalLowPrice = double.MaxValue;
            double prevLocalLowRSI = double.MaxValue;
            
            int lowsFound = 0;
            for (int i = index; i >= index - DIV_LEN + 1 && i >= 1; i--)
            {
                if (Low(i) < Low(i + 1) && Low(i) <= Low(i - 1))
                {
                    double rsiAtLow = RSI(i);
                    if (lowsFound == 0)
                    {
                        localLowPrice = Low(i);
                        localLowRSI = rsiAtLow;
                        priceBarLow = i;
                    }
                    else if (lowsFound == 1)
                    {
                        prevLocalLowPrice = Low(i);
                        prevLocalLowRSI = rsiAtLow;
                    }
                    lowsFound++;
                    if (lowsFound >= 2) break;
                }
            }
            
            _bullRsiDiv = lowsFound >= 2 && priceBarLow < index - 2 && localLowPrice > prevLocalLowPrice && localLowRSI < prevLocalLowRSI;
            
            double localHighPrice = double.MinValue;
            double localHighRSI = double.MinValue;
            int priceBarHigh = 0;
            double prevLocalHighPrice = double.MinValue;
            double prevLocalHighRSI = double.MinValue;
            
            int highsFound = 0;
            for (int i = index; i >= index - DIV_LEN + 1 && i >= 1; i--)
            {
                if (High(i) > High(i + 1) && High(i) >= High(i - 1))
                {
                    double rsiAtHigh = RSI(i);
                    if (highsFound == 0)
                    {
                        localHighPrice = High(i);
                        localHighRSI = rsiAtHigh;
                        priceBarHigh = i;
                    }
                    else if (highsFound == 1)
                    {
                        prevLocalHighPrice = High(i);
                        prevLocalHighRSI = rsiAtHigh;
                    }
                    highsFound++;
                    if (highsFound >= 2) break;
                }
            }
            
            _bearRsiDiv = highsFound >= 2 && priceBarHigh < index - 2 && localHighPrice < prevLocalHighPrice && localHighRSI > prevLocalHighRSI;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — EQH / EQL
        //=============================================================================
        
        private void UpdateEQHEQL(int index)
        {
            if (index < 20) return;
            double eqBuffer = _adaptiveATR * 0.25;
            
            _equalHighs = false;
            _equalLows = false;
            
            for (int i = 10; i <= 15; i++)
            {
                if (index > i && Math.Abs(High(index) - High(index - i)) < eqBuffer &&
                    High(index) >= High(index - i)) _equalHighs = true;
                if (index > i && Math.Abs(Low(index) - Low(index - i)) < eqBuffer &&
                    Low(index) <= Low(index - i)) _equalLows = true;
            }
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — FVG
        //=============================================================================
        
        private void UpdateFVG(int index)
        {
            _fvgBullUp = 0.0; _fvgBullDown = 0.0;
            _fvgBearUp = 0.0; _fvgBearDown = 0.0;
            _fvg = double.NaN;
            
            if (index < 3) return;
            double buffer = _adaptiveATR * 0.15;
            
            for (int i = 2; i <= Math.Min(10, index - 2); i++)
            {
                double gapLow = Low(index - i - 2);
                double gapHigh = High(index - i);
                if (gapHigh < gapLow && (gapLow - gapHigh) > buffer)
                {
                    _fvgBullUp = gapHigh;
                    _fvgBullDown = gapLow;
                    if (High(index) >= gapHigh && Low(index) <= gapLow)
                        _fvg = gapLow;
                    break;
                }
            }
            
            for (int i = 2; i <= Math.Min(10, index - 2); i++)
            {
                double gapLow = Low(index - i);
                double gapHigh = High(index - i - 2);
                if (gapHigh < gapLow && (gapLow - gapHigh) > buffer)
                {
                    _fvgBearUp = gapHigh;
                    _fvgBearDown = gapLow;
                    if (High(index) >= gapHigh && Low(index) <= gapLow)
                        _fvg = gapHigh;
                    break;
                }
            }
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Order Blocks
        //=============================================================================
        
        private void UpdateOrderBlocks(int index)
        {
            if (index < 10) return;
            double obBuffer = _adaptiveATR * 0.25;
            double body = Math.Abs(Close(index - 1) - Open(index - 1));
            double wickTop = High(index - 1) - Math.Max(Open(index - 1), Close(index - 1));
            double wickBot = Math.Min(Open(index - 1), Close(index - 1)) - Low(index - 1);
            double bodyPct = body / (High(index - 1) - Low(index - 1) + 0.0001) * 100.0;
            
            if (Close(index - 1) < Open(index - 1) && bodyPct > 40)
            {
                _obBullHigh = High(index - 1);
                _obBullLow = Low(index - 1);
                bool engulfed = Open(index) <= High(index - 1) && Close(index) >= Low(index - 1);
                _obBull = engulfed && index >= _lastObBullBar + 5;
                if (_obBull) _lastObBullBar = index;
            }
            else _obBull = false;
            
            if (Close(index - 1) > Open(index - 1) && bodyPct > 40)
            {
                _obBearHigh = High(index - 1);
                _obBearLow = Low(index - 1);
                bool engulfed = Open(index) >= Low(index - 1) && Close(index) <= High(index - 1);
                _obBear = engulfed && index >= _lastObBearBar + 5;
                if (_obBear) _lastObBearBar = index;
            }
            else _obBear = false;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Support & Resistance
        //=============================================================================
        
        private void UpdateSR(int index)
        {
            if (index < SR_SCAN_LEN) return;
            
            double buffer = _adaptiveATR * SR_BUFFER_MULTIPLIER;
            var levels = new List<double>();
            var counts = new List<int>();
            
            for (int i = 1; i <= SR_SCAN_LEN && index > i; i++)
            {
                double target = High(index - i);
                bool found = false;
                for (int j = 0; j < levels.Count; j++)
                {
                    if (Math.Abs(levels[j] - target) < buffer)
                    {
                        counts[j]++;
                        found = true;
                        break;
                    }
                }
                if (!found) { levels.Add(target); counts.Add(1); }
                
                target = Low(index - i);
                found = false;
                for (int j = 0; j < levels.Count; j++)
                {
                    if (Math.Abs(levels[j] - target) < buffer)
                    {
                        counts[j]++;
                        found = true;
                        break;
                    }
                }
                if (!found) { levels.Add(target); counts.Add(1); }
            }
            
            _srLevels.Clear();
            _srResistances.Clear();
            _srSupports.Clear();
            for (int i = 0; i < levels.Count; i++)
            {
                if (counts[i] >= SR_REPEAT_MIN)
                {
                    _srLevels.Add(levels[i]);
                    if (levels[i] > Close(index)) _srResistances.Add(levels[i]);
                    else _srSupports.Add(levels[i]);
                }
            }
            
            _srResistances.Sort();
            _srSupports.Sort((a, b) => b.CompareTo(a));
            
            _srNearestRes = _srResistances.Count > 0 ? _srResistances[0] : double.NaN;
            _srNearestSup = _srSupports.Count > 0 ? _srSupports[0] : double.NaN;
            _srNearestRes2 = _srResistances.Count > 1 ? _srResistances[1] : double.NaN;
            _srNearestSup2 = _srSupports.Count > 1 ? _srSupports[1] : double.NaN;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — VPVR
        //=============================================================================
        
        private void UpdateVPVR(int index)
        {
            if (index < VP_LOOKBACK) return;
            
            double peak = double.MinValue; double trough = double.MaxValue;
            int startBar = Math.Min(index, VP_LOOKBACK);
            for (int i = index - startBar + 1; i <= index; i++)
            {
                if (High(i) > peak) peak = High(i);
                if (Low(i) < trough) trough = Low(i);
            }
            double vpRange = peak - trough;
            double bucketSize = vpRange / VP_BUCKETS;
            if (bucketSize <= 0) bucketSize = _adaptiveATR * 0.25;
            
            double[] vpVolume = new double[VP_BUCKETS];
            for (int i = index - startBar + 1; i <= index; i++)
            {
                double mid = (High(i) + Low(i)) / 2.0;
                int idx = (int)Math.Floor((mid - trough) / bucketSize);
                idx = Math.Max(0, Math.Min(VP_BUCKETS - 1, idx));
                vpVolume[idx] += Volume(i);
            }
            
            double maxVol = 0;
            for (int i = 0; i < VP_BUCKETS; i++)
                if (vpVolume[i] > maxVol) maxVol = vpVolume[i];
            
            int pocIdx = 0;
            for (int i = 0; i < VP_BUCKETS; i++)
            {
                if (vpVolume[i] >= maxVol)
                {
                    _vpvrPOC = trough + i * bucketSize + bucketSize / 2.0;
                    pocIdx = i;
                    break;
                }
            }
            
            double cumVol = 0; double totalVol = 0;
            for (int i = 0; i < VP_BUCKETS; i++) totalVol += vpVolume[i];
            double valTarget = totalVol * 0.68 / 2.0;
            
            for (int i = pocIdx; i >= 0; i--)
            {
                cumVol += vpVolume[i];
                if (cumVol >= valTarget) { _vpvrVAL = trough + i * bucketSize + bucketSize / 2.0; break; }
            }
            cumVol = 0;
            for (int i = pocIdx; i < VP_BUCKETS; i++)
            {
                cumVol += vpVolume[i];
                if (cumVol >= valTarget) { _vpvrVAH = trough + i * bucketSize + bucketSize / 2.0; break; }
            }
            
            if (Close(index) > _vpvrVAH && Close(index - 1) <= _vpvrVAH)
                _vpvrBreakUp = true;
            else if (Close(index) < _vpvrVAL && Close(index - 1) >= _vpvrVAL)
                _vpvrBreakDown = true;
            else { _vpvrBreakUp = false; _vpvrBreakDown = false; }
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Liquidity
        //=============================================================================
        
        private void UpdateLiquidity(int index)
        {
            if (index < 50) return;
            double extBuffer = _adaptiveATR * 0.50;
            
            double recentHigh = High(index - 1); double recentLow = Low(index - 1);
            for (int i = 1; i <= 20; i++)
            {
                if (High(index - i) > recentHigh) recentHigh = High(index - i);
                if (Low(index - i) < recentLow) recentLow = Low(index - i);
            }
            
            _liquidityAbove = double.NaN;
            for (int i = 1; i <= 50 && index > i + 1; i++)
            {
                if (High(index - i) > High(index - i + 1) && High(index - i) > High(index - i - 1)
                    && Math.Abs(High(index - i) - recentHigh) > extBuffer)
                {
                    _liquidityAbove = High(index - i);
                    break;
                }
            }
            
            _liquidityBelow = double.NaN;
            for (int i = 1; i <= 50 && index > i + 1; i++)
            {
                if (Low(index - i) < Low(index - i + 1) && Low(index - i) < Low(index - i - 1)
                    && Math.Abs(Low(index - i) - recentLow) > extBuffer)
                {
                    _liquidityBelow = Low(index - i);
                    break;
                }
            }
            
            _liquiditySweptAbove = !double.IsNaN(_liquidityAbove) && High(index) >= _liquidityAbove;
            _liquiditySweptBelow = !double.IsNaN(_liquidityBelow) && Low(index) <= _liquidityBelow;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Climax
        //=============================================================================
        
        private void UpdateClimax(int index)
        {
            _climaxVolReversal = false;
            _climaxRange = false;
            
            if (index < 5) return;
            
            double volSma = _volSma20;
            double volThreshold = volSma * 2.0;
            double rangeATR = High(index) - Low(index);
            
            if (Volume(index) > volThreshold && rangeATR > _adaptiveATR * 1.5 &&
                Close(index) < Open(index) && Close(index - 1) > Open(index - 1))
                _climaxVolReversal = true;
            
            if (rangeATR > _adaptiveATR * 2.0 &&
                Volume(index) > _volSma20 * 1.5)
                _climaxRange = true;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Regime
        //=============================================================================
        
        private void UpdateRegimeEngine()
        {
            int bulls = 0, bears = 0;
            
            if (_regimeTrending && _bullTrend) bulls++;
            if (_mrBullScore > _mrBearScore) bulls++;
            if (_bullBOS || _displacementBull) bulls++;
            if (_chochBull) bulls++;
            if (_fvgBullUp > 0) bulls++;
            if (_obBull) bulls++;
            if (_bullRsiDiv) bulls++;
            if (_equalHighs) bulls++;
            if (_sessionQuality >= 70 && !_inOffHours) bulls++;
            
            if (_regimeTrending && _bearTrend) bears++;
            if (_mrBearScore > _mrBullScore) bears++;
            if (_bearBOS || _displacementBear) bears++;
            if (_chochBear) bears++;
            if (_fvgBearUp > 0) bears++;
            if (_obBear) bears++;
            if (_bearRsiDiv) bears++;
            if (_equalLows) bears++;
            if (_sessionQuality >= 70 && !_inOffHours) bears++;
            
            int total = bulls + bears;
            _regimeDominant = total > 0 ? (double)bulls / total * 100.0 : 50.0;
            _regimeBias = bulls > bears ? "LONG" : bears > bulls ? "SHORT" : "NEUTRAL";
            _regimeConviction = Math.Min(100, Math.Max(0, (Math.Abs(bulls - bears) + 1) * 10));
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Probability (Bayesian Softmax)
        //=============================================================================
        
        private void UpdateProbabilityEngine()
        {
            double bullEvidence = 0.0, bearEvidence = 0.0;
            
            bullEvidence += (_bullTrendScore / 100.0) * 0.20;
            bearEvidence += (_bearTrendScore / 100.0) * 0.20;
            
            double mrContra = (_mrComposite < 0 ? -_mrComposite : 0) / 100.0;
            double mrPro = (_mrComposite > 0 ? _mrComposite : 0) / 100.0;
            bullEvidence += mrContra * 0.10;
            bearEvidence += mrPro * 0.10;
            
            if (_regimeTrending)
            {
                bullEvidence += _bullTrend ? 0.10 : 0.0;
                bearEvidence += _bearTrend ? 0.10 : 0.0;
            }
            if (_regimeRanging)
            {
                bullEvidence += mrContra * 0.10;
                bearEvidence += mrPro * 0.10;
            }
            
            bullEvidence += _bullBOS ? 0.15 : 0.0;
            bearEvidence += _bearBOS ? 0.15 : 0.0;
            
            bullEvidence += _displacementBull ? 0.10 : 0.0;
            bearEvidence += _displacementBear ? 0.10 : 0.0;
            
            bullEvidence += _bullRsiDiv ? 0.12 : 0.0;
            bearEvidence += _bearRsiDiv ? 0.12 : 0.0;
            
            bullEvidence += _fvg > 0 && _fvg == _fvgBullDown ? 0.08 : 0.0;
            bearEvidence += _fvg > 0 && _fvg == _fvgBearUp ? 0.08 : 0.0;
            
            bullEvidence += _obBull ? 0.08 : 0.0;
            bearEvidence += _obBear ? 0.08 : 0.0;
            
            bullEvidence += _inKillzone ? 0.10 : 0.0;
            bearEvidence += _inKillzone ? 0.10 : 0.0;
            
            double cfLongProb = CumulativeNormal(_retCFZ);
            bullEvidence += cfLongProb * 0.12;
            bearEvidence += (1.0 - cfLongProb) * 0.12;
            
            double tau = SoftmaxTemperature;
            double expBull = Math.Exp(bullEvidence / tau);
            double expBear = Math.Exp(bearEvidence / tau);
            double sumExp = expBull + expBear;
            
            _bullProb = sumExp > 0 ? expBull / sumExp * 100.0 : 50.0;
            _bearProb = sumExp > 0 ? expBear / sumExp * 100.0 : 50.0;
            _probDominant = Math.Max(_bullProb, _bearProb);
            _probSpread = Math.Abs(_bullProb - _bearProb);
        }
        
        private static double CumulativeNormal(double x)
        {
            double a1 = 0.254829592, a2 = -0.284496736, a3 = 1.421413741;
            double a4 = -1.453152027, a5 = 1.061405429, p = 0.3275911;
            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x) / Math.Sqrt(2.0);
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
            return 0.5 * (1.0 + sign * y);
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Historical Analog
        //=============================================================================
        
        private void UpdateHistoricalAnalog(int index)
        {
            _analogActive = false;
            _analogOutcomes.Clear();
            _analogFrequencies.Clear();
            
            if (index < OutcomeN + 10 || !EnableAnalogEngine) return;
            
            double analogRet = _goldRet;
            double analogATR = _adaptiveATR;
            double analogVol = _volSma20 > 0 ? Volume(index) / _volSma20 : 1.0;
            double analogRange = _adaptiveATR > 0 ? (High(index) - Low(index)) / _adaptiveATR : 1.0;
            
            int scanBars = Math.Min(MAX_SCAN_BARS, index - OutcomeN);
            int bestIdx = -1; double bestScore = double.MaxValue;
            double eps = 0.0001;
            
            for (int i = 1; i <= scanBars; i++)
            {
                double retDiff = Math.Abs(_goldRetArray[i] - analogRet) / (Math.Abs(analogRet) + eps);
                double atrDiff = Math.Abs(_atrArray[i] - _adaptiveATR) / (_adaptiveATR + eps);
                double volDiff = Math.Abs(_volArray[i] / Math.Max(_volSma20, 1.0) - analogVol) / (analogVol + eps);
                double rangeDiff = Math.Abs(_rangeArray[i] - analogRange) / (analogRange + eps);
                
                double simScore = 100.0 - (retDiff * 40.0 + atrDiff * 25.0 + volDiff * 20.0 + rangeDiff * 15.0);
                
                if (simScore > SIM_THRESHOLD && simScore < bestScore)
                {
                    bestScore = simScore;
                    bestIdx = i;
                }
            }
            
            if (bestIdx > 0 && bestIdx + OutcomeN <= HIST_MAX && index > bestIdx)
            {
                _analogActive = true;
                _analogBestIdx = bestIdx;
                _analogSimScore = bestScore;
                
                _analogPrevOC = _goldRetArray[bestIdx];
                int outcomeStart = bestIdx - 1;
                
                for (int k = 0; k < OutcomeN; k++)
                {
                    if (outcomeStart - k > 0 && outcomeStart - k < HIST_MAX)
                    {
                        _analogOutcomes.Add(_goldRetArray[outcomeStart - k]);
                    }
                }
                
                double prevCloseAtMatch = Close(bestIdx);
                double currClose = Close(index);
                _analogPRevROC = prevCloseAtMatch > 0 ? (currClose - prevCloseAtMatch) / prevCloseAtMatch * 100.0 : 0.0;
                _analogConf = Math.Min(100.0, bestScore + 10.0);
            }
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Scenario
        //=============================================================================
        
        private void UpdateScenario()
        {
            _scenarioBullCount = 0; _scenarioBearCount = 0;
            
            if (_bullBOS || _swingBosLabelBull) _scenarioBullCount++;
            if (_bullRsiDiv) _scenarioBullCount += 2;
            if (_displacementBull) _scenarioBullCount++;
            if (_obBull) _scenarioBullCount++;
            if (_fvgBullUp > 0) _scenarioBullCount++;
            if (_chochBull) _scenarioBullCount += 2;
            if (_equalHighs) _scenarioBullCount++;
            if (_mssBull) _scenarioBullCount += 3;
            if (_liquiditySweptBelow) _scenarioBullCount += 2;
            if (_climaxVolReversal) _scenarioBullCount += 2;
            
            if (_bearBOS || _swingBosLabelBear) _scenarioBearCount++;
            if (_bearRsiDiv) _scenarioBearCount += 2;
            if (_displacementBear) _scenarioBearCount++;
            if (_obBear) _scenarioBearCount++;
            if (_fvgBearUp > 0) _scenarioBearCount++;
            if (_chochBear) _scenarioBearCount += 2;
            if (_equalLows) _scenarioBearCount++;
            if (_mssBear) _scenarioBearCount += 3;
            if (_liquiditySweptAbove) _scenarioBearCount += 2;
            if (_climaxVolReversal) _scenarioBearCount += 2;
            
            _scenarioBias = _scenarioBullCount > _scenarioBearCount ? "LONG" :
                _scenarioBearCount > _scenarioBullCount ? "SHORT" : "NEUTRAL";
            _scenarioConviction = Math.Max(_scenarioBullCount, _scenarioBearCount);
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Signal Quality
        //=============================================================================
        
        private void UpdateSignalQuality(int index)
        {
            double q = 5.0;
            
            // HTF alignment not available in single-TF cTrader mode; default to 5.0
            q += _regimeConviction * 0.15;
            q += _sessionQuality * 0.10;
            q += _probSpread * 0.20;
            q += _analogConf * 0.10;
            q += _recentBars ? 10.0 : 0.0;
            q += (_bullBOS || _bearBOS) ? 10.0 : 0.0;
            q += (_scenarioBullCount > 0 || _scenarioBearCount > 0) ? 10.0 : 0.0;
            
            double distToPOC = Math.Abs(Close(index) - _vpvrPOC) / _adaptiveATR;
            q += Math.Max(0, 10.0 - distToPOC * 2.0);
            
            _signalQuality = Math.Min(100, Math.Max(0, q));
            
            _entryGrade = _signalQuality >= 80 ? "A" : _signalQuality >= 65 ? "B" :
                _signalQuality >= 50 ? "C" : _signalQuality >= 35 ? "D" : "F";
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Statistical Engines
        //=============================================================================
        
        private void RunStatsEngines(int index)
        {
            if (index == _lastStatsBar) return;
            _lastStatsBar = index;
            
            if (!EnableStatsEngine) return;
            if (!UseThrottle || index - _lastStatsRunBar >= 20)
            {
                _lastStatsRunBar = index;
                ComputeFullAnalogPass(index);
            }
        }
        
        private void ComputeFullAnalogPass(int index)
        {
            int outcomeN = OutcomeN;
            int maxScan = Math.Min(index - outcomeN - 5, HistMax);
            if (maxScan < 20) return;
            
            var outcomes = new List<double>();
            int validMatches = 0;
            
            double curRet = _goldRet;
            double curAtr = _adaptiveATR;
            double curVol = _volSma20 > 0 ? Volume(index) / _volSma20 : 1.0;
            double curRange = _adaptiveATR > 0 ? (High(index) - Low(index)) / _adaptiveATR : 1.0;
            double eps = 0.0001;
            
            for (int i = 1; i <= maxScan; i++)
            {
                double retDiff = Math.Abs(_goldRetArray[i] - curRet) / (Math.Abs(curRet) + eps);
                double atrDiff = Math.Abs(_atrArray[i] - curAtr) / (curAtr + eps);
                double volDiff = Math.Abs(_volArray[i] / Math.Max(_volSma20, 1.0) - curVol) / (curVol + eps);
                double rangeDiff = Math.Abs(_rangeArray[i] - curRange) / (curRange + eps);
                double sim = 100.0 - (retDiff * 40 + atrDiff * 25 + volDiff * 20 + rangeDiff * 15);
                
                if (sim > SIM_THRESHOLD && i > outcomeN)
                {
                    double fwdRet = _goldRetArray[i - 1];
                    outcomes.Add(fwdRet);
                    validMatches++;
                }
            }
            
            _statMatchCount = validMatches;
            _statWinCount = 0; _statLossCount = 0;
            _statTotalRet = 0.0;
            _statAvgWin = 0.0; _statAvgLoss = 0.0;
            _statWinRate = 50.0;
            
            if (outcomes.Count > 0)
            {
                foreach (double r in outcomes)
                {
                    _statTotalRet += r;
                    if (r > 0) _statWinCount++;
                    else _statLossCount++;
                }
                int total = outcomes.Count;
                _statWinRate = (double)_statWinCount / total * 100.0;
                _statAvgRet = _statTotalRet / total;
                _statAvgWin = _statWinCount > 0 ? _statTotalRet / _statWinCount : 0.0;
                _statAvgLoss = _statLossCount > 0 ? (_statTotalRet - _statAvgRet * total) / _statLossCount : 0.0;
            }
            
            _probAdjusted = _statWinRate;
        }
        
        //=============================================================================
        // REGION: ENGINE METHODS — Trade Plan
        //=============================================================================
        
        private void UpdateTradePlan(int index)
        {
            _shouldBuy = false; _shouldSell = false;
            _tpIsLong = false;
            
            if (index == _lastSignalBar) return;
            
            if (_entryGrade == "F" || _entryGrade == "D")
            {
                _lastSignalGrade = _entryGrade;
                return;
            }
            
            bool longOk = _bullProb >= MinProbThreshold && _regimeBias == "LONG"
                && _scenarioBias == "LONG" && _signalQuality >= MinSignalQuality;
            bool shortOk = _bearProb >= MinProbThreshold && _regimeBias == "SHORT"
                && _scenarioBias == "SHORT" && _signalQuality >= MinSignalQuality;
            
            if (longOk)
            {
                _shouldBuy = true; _tpIsLong = true; _lastSignalBar = index;
                _lastSignalGrade = _entryGrade;
            }
            else if (shortOk)
            {
                _shouldSell = true; _tpIsLong = false; _lastSignalBar = index;
                _lastSignalGrade = _entryGrade;
            }
            
            _tpEntry = Close(index);
            _tpSL = _tpIsLong ? _tpEntry - _adaptiveATR * 1.5 : _tpEntry + _adaptiveATR * 1.5;
            _tp1 = _tpIsLong ? _tpEntry + _adaptiveATR : _tpEntry - _adaptiveATR;
            _tp2 = _tpIsLong ? _tpEntry + _adaptiveATR * 2.0 : _tpEntry - _adaptiveATR * 2.0;
            _tpRR1 = _adaptiveATR > 0 ? (_tp1 - _tpEntry) / (_tpEntry - _tpSL) * (_tpIsLong ? 1 : -1) : 0.0;
            
            _tpDirStr = _tpIsLong ? "BUY" : "SELL";
            _tpEntryStr = _tpEntry.ToString("F1");
            _tpSLStr = _tpSL.ToString("F1");
            _tp1Str = _tp1.ToString("F1");
            _tp2Str = _tp2.ToString("F1");
            _tpRRStr = _tpRR1.ToString("F2");
        }
        
        //=============================================================================
        // REGION: HELPER FUNCTIONS
        //=============================================================================
        
        private static double Nz(double val, double fallback = 0.0)
        {
            return double.IsNaN(val) || double.IsInfinity(val) ? fallback : val;
        }
        
        private double PercentRank(double value, int period, int index)
        {
            if (index < period) return 50.0;
            int countBelow = 0;
            int countEqual = 0;
            for (int i = 0; i < period && i <= index; i++)
            {
                if (double.IsNaN(_atrArray[i])) continue;
                if (_atrArray[i] < value) countBelow++;
                else if (Math.Abs(_atrArray[i] - value) < 0.000001) countEqual++;
            }
            int count = Math.Min(period, index + 1);
            return count > 0 ? (countBelow + 0.5 * countEqual) / count * 100.0 : 50.0;
        }
        
        private double StdDev(DataSeries source, int period, int index)
        {
            if (index < period) return 0.0;
            double mean = SMA(source, period, index);
            double sumSq = 0.0;
            int count = 0;
            for (int i = index - period + 1; i <= index; i++)
            {
                double v = source[i];
                double diff = v - mean;
                sumSq += diff * diff;
                count++;
            }
            return count > 0 ? Math.Sqrt(sumSq / count) : 0.0;
        }
        
        private double StdDev(double value, int period, int index)
        {
            if (index < period) return 0.0;
            double mean = SMA(value, period, index);
            double sumSq = 0.0;
            int count = 0;
            for (int i = index - period + 1; i <= index; i++)
            {
                double v = double.IsNaN(_goldRetArray[i]) ? 0.0 : _goldRetArray[i];
                double diff = v - mean;
                sumSq += diff * diff;
                count++;
            }
            return count > 0 ? Math.Sqrt(sumSq / count) : 0.0;
        }
        
        private double SMA(double value, int period, int index)
        {
            if (index < period - 1) return 0.0;
            double sum = 0.0;
            int count = 0;
            for (int i = index - period + 1; i <= index; i++)
            {
                double v = double.IsNaN(_goldRetArray[i]) ? 0.0 : _goldRetArray[i];
                sum += v;
                count++;
            }
            return count > 0 ? sum / count : 0.0;
        }
        
        //=============================================================================
        // REGION: DASHBOARD & CHART DRAWING
        //=============================================================================
        
        private void UpdateDashboard()
        {
            const string dashKey = "Q3_Dashboard";
            Chart.RemoveObject(dashKey);
            
            if (!ShowDashboard) return;
            
            string nl = Environment.NewLine;
            string dash =
                "═══ XAUUSD Quantum 3.0 ═══" + nl +
                "──────── SESSION ────────" + nl +
                $"  Session : {_sessionLabel}" + nl +
                $"  Quality : {_sessionQuality}%" + nl +
                $"  Killzone: {(_inKillzone ? "YES" : "no")}" + nl +
                "──────── TREND ──────────" + nl +
                $"  ADX     : {_adxVal:F1}" + nl +
                $"  D+/D-   : {_diPlus:F1}/{_diMinus:F1}" + nl +
                $"  Regime  : {(_regimeTrending ? "TRENDING" : _regimeRanging ? "RANGING" : "DEAD")}" + nl +
                "──────── MR ─────────────" + nl +
                $"  MR Comp : {_mrComposite:F0}" + nl +
                $"  Regime  : {_mrRegime}" + nl +
                $"  CF-Z    : {_retCFZ:F2}" + nl +
                "──────── STRUCTURE ──────" + nl +
                $"  BOS     : {(_bullBOS ? "BULL" : _bearBOS ? "BEAR" : "—")}" + nl +
                $"  CHOCH   : {(_chochBull ? "BULL" : _chochBear ? "BEAR" : "—")}" + nl +
                $"  MSS     : {(_mss ? "YES" : "no")}" + nl +
                "──────── PROBABILITY ────" + nl +
                $"  Bull    : {_bullProb:F1}%" + nl +
                $"  Bear    : {_bearProb:F1}%" + nl +
                $"  Spread  : {_probSpread:F1}%" + nl +
                "──────── SIGNAL ─────────" + nl +
                $"  Quality : {_signalQuality:F0}" + nl +
                $"  Grade   : {_entryGrade}" + nl +
                "──────── TRADE PLAN ─────" + nl +
                $"  Dir     : {_tpDirStr}" + nl +
                $"  Entry   : {_tpEntryStr}" + nl +
                $"  SL      : {_tpSLStr}" + nl +
                $"  TP1     : {_tp1Str} (RR {_tpRRStr})" + nl +
                $"  TP2     : {_tp2Str}" + nl +
                "──────── ANALOG ─────────" + nl +
                $"  Matches : {_statMatchCount}" + nl +
                $"  WinRate : {_statWinRate:F1}%" + nl +
                $"  Conf    : {_analogConf:F0}%";
            
            Chart.DrawStaticText(dashKey, dash, VerticalAlignment.Top, HorizontalAlignment.Left, Color.White);
        }
        
        private void UpdateChartDrawing()
        {
            // Remove stale drawings
            Chart.RemoveObject("Q3_VPVR_POC");
            Chart.RemoveObject("Q3_VPVR_VAH");
            Chart.RemoveObject("Q3_VPVR_VAL");
            Chart.RemoveObject("Q3_EMA20");
            Chart.RemoveObject("Q3_EMA100");
            Chart.RemoveObject("Q3_EMA200");
            Chart.RemoveObject("Q3_TP_ENTRY");
            Chart.RemoveObject("Q3_TP_SL");
            Chart.RemoveObject("Q3_TP_TP1");
            Chart.RemoveObject("Q3_TP_TP2");
            Chart.RemoveObject("Q3_FVG_BULL");
            Chart.RemoveObject("Q3_FVG_BEAR");
            Chart.RemoveObject("Q3_BOS_BULL");
            Chart.RemoveObject("Q3_BOS_BEAR");
            
            if (!ShowSignals) return;
            
            // Clean up any stale S/R level lines
            for (int i = 0; i < 20; i++)
                Chart.RemoveObject("Q3_SR_" + i);
            
            int lastIdx = Bars.Count - 1;
            DateTime t0 = Time(lastIdx);
            DateTime t1 = Time(Math.Max(0, lastIdx - 5));
            
            //--- EMAs ---
            if (ShowEMAs)
            {
                Chart.DrawLine("Q3_EMA20", t0, _ema20, t1, _ema20, Color.FromArgb(180, 100, 149, 237), 1);
                Chart.DrawLine("Q3_EMA100", t0, _ema100, t1, _ema100, Color.FromArgb(180, 255, 165, 0), 1);
                Chart.DrawLine("Q3_EMA200", t0, _ema200, t1, _ema200, Color.FromArgb(180, 220, 20, 60), 1);
            }
            
            //--- S/R Levels ---
            if (ShowSR)
            {
                for (int i = 0; i < _srLevels.Count && i < 10; i++)
                {
                    string key = "Q3_SR_" + i;
                    Chart.DrawLine(key, t0, _srLevels[i], t1, _srLevels[i],
                        Color.FromArgb(100, 200, 200, 200), 1);
                }
            }
            
            //--- VPVR ---
            if (!double.IsNaN(_vpvrPOC) && _vpvrPOC > 0)
            {
                Chart.DrawLine("Q3_VPVR_POC", t0, _vpvrPOC, t1, _vpvrPOC,
                    Color.FromArgb(200, 128, 0, 128), 2);
                if (!double.IsNaN(_vpvrVAH))
                    Chart.DrawLine("Q3_VPVR_VAH", t0, _vpvrVAH, t1, _vpvrVAH,
                        Color.FromArgb(150, 0, 255, 255), 1);
                if (!double.IsNaN(_vpvrVAL))
                    Chart.DrawLine("Q3_VPVR_VAL", t0, _vpvrVAL, t1, _vpvrVAL,
                        Color.FromArgb(150, 0, 255, 255), 1);
            }
            
            //--- FVG Boxes ---
            if (ShowFVG)
            {
                if (_fvgBullUp > 0 && _fvgBullDown > 0)
                {
                    Chart.DrawRectangle("Q3_FVG_BULL", t0, _fvgBullUp, t1, _fvgBullDown,
                        Color.FromArgb(120, 0, 200, 0), 1);
                }
                if (_fvgBearUp > 0 && _fvgBearDown > 0)
                {
                    Chart.DrawRectangle("Q3_FVG_BEAR", t0, _fvgBearUp, t1, _fvgBearDown,
                        Color.FromArgb(120, 200, 0, 0), 1);
                }
            }
            
            //--- BOS Labels ---
            if (ShowSignals)
            {
                if (_bosLabelBull)
                    Chart.DrawText("Q3_BOS_BULL", "BOS ▲", t0, _activeResistance,
                        Color.FromArgb(200, 0, 255, 0));
                if (_bosLabelBear)
                    Chart.DrawText("Q3_BOS_BEAR", "BOS ▼", t0, _activeSupport,
                        Color.FromArgb(200, 255, 0, 0));
            }
            
            //--- Trade Plan Lines ---
            if (ShowTradePlan && (_shouldBuy || _shouldSell))
            {
                double entry = _tpEntry, sl = _tpSL, tp1 = _tp1, tp2 = _tp2;
                Color lineCol = _tpIsLong ? Color.FromArgb(200, 0, 200, 0) : Color.FromArgb(200, 200, 0, 0);
                Color slCol = Color.FromArgb(200, 255, 0, 0);
                
                Chart.DrawLine("Q3_TP_ENTRY", t0, entry, t1, entry, lineCol, 2);
                Chart.DrawLine("Q3_TP_SL", t0, sl, t1, sl, slCol, 1);
                Chart.DrawLine("Q3_TP_TP1", t0, tp1, t1, tp1, lineCol, 1);
                Chart.DrawLine("Q3_TP_TP2", t0, tp2, t1, tp2, Color.FromArgb(150, 255, 255, 0), 1);
            }
        }
    } // End of class XAUUSDQuantum3
} // End of namespace cAlgo.Indicators