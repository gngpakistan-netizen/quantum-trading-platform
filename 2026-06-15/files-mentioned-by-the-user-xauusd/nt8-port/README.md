# XAUUSD Quantum 3.0

Multi-factor statistical trading framework for XAUUSD.

Ported from Pine Script v6 (TradingView) to cTrader Automate (C#).

## Disclaimer

"Score" values are ordinal evidence-weighted composites, not statistically validated probabilities. Use for directional bias only, not for position sizing. Past outcomes do not guarantee future results.

## Installation

1. Copy `XAUUSD_Quantum_3.cs` to:
   `%USERPROFILE%\Documents\cTrader\Algo\cAlgo\Indicators\`
2. Open cTrader → Algo → Build
3. Attach to an XAUUSD chart

## Parameters

See the `[Parameter]` attributes in the source file for full configuration.

## Key Features

- Bayesian softmax probability engine (replaces ad-hoc normalizations)
- Adaptive ATR regime detection
- Cornish-Fisher adjusted return distribution estimates
- Multi-session DST-aware scheduling
- Historical analog similarity matching
- VPVR / order block / FVG / liquidity detection
- Signal quality grading (A–F)
