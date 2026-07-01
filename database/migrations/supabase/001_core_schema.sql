-- XAUUSD Quantum Platform — Supabase Database Schema
-- Migration 001: Core Tables
-- Run this in Supabase SQL Editor or via `supabase db push`

-- ============================================================
-- EXTENSIONS
-- ============================================================
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- ENUMS
-- ============================================================
CREATE TYPE signal_direction AS ENUM ('long', 'short', 'neutral');
CREATE TYPE trade_status AS ENUM ('open', 'closed', 'cancelled');
CREATE TYPE exit_reason AS ENUM ('sl_hit', 'tp1_hit', 'tp2_hit', 'manual', 'signal_reversal', 'timeout');
CREATE TYPE validation_stream AS ENUM ('mathematical', 'strategy', 'dashboard', 'statistical', 'timing');
CREATE TYPE audit_dimension AS ENUM ('mathematical', 'statistical', 'dashboard', 'timing', 'risk', 'code_quality');
CREATE TYPE todo_status AS ENUM ('pending', 'in_progress', 'completed', 'cancelled');
CREATE TYPE requirement_status AS ENUM ('draft', 'specified', 'implemented', 'validated', 'audited', 'operational');
CREATE TYPE regime_type AS ENUM ('trending_up', 'trending_down', 'ranging', 'volatile');

-- ============================================================
-- MARKET DATA
-- ============================================================
CREATE TABLE bars (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    symbol TEXT NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL,
    timeframe TEXT NOT NULL,
    open DOUBLE PRECISION NOT NULL,
    high DOUBLE PRECISION NOT NULL,
    low DOUBLE PRECISION NOT NULL,
    close DOUBLE PRECISION NOT NULL,
    volume DOUBLE PRECISION NOT NULL DEFAULT 0,
    tick_volume DOUBLE PRECISION,
    spread DOUBLE PRECISION,
    vwap DOUBLE PRECISION,
    is_complete BOOLEAN DEFAULT FALSE,
    source TEXT DEFAULT 'tradingview',
    quality_score DOUBLE PRECISION DEFAULT 1.0,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_bars_symbol_timeframe ON bars (symbol, timeframe, timestamp DESC);

CREATE TABLE ticks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    symbol TEXT NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL,
    price DOUBLE PRECISION NOT NULL,
    volume DOUBLE PRECISION,
    bid DOUBLE PRECISION,
    ask DOUBLE PRECISION,
    source TEXT DEFAULT 'tradingview',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_ticks_symbol_time ON ticks (symbol, timestamp DESC);

-- ============================================================
-- FEATURE STORE
-- ============================================================
CREATE TABLE feature_sets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    symbol TEXT NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL,
    timeframe TEXT NOT NULL,
    bar_id UUID REFERENCES bars(id),

    -- Trend
    adx DOUBLE PRECISION DEFAULT 0,
    adx_direction TEXT DEFAULT 'neutral',
    ema50 DOUBLE PRECISION DEFAULT 0,
    ema200 DOUBLE PRECISION DEFAULT 0,
    ema_spread DOUBLE PRECISION DEFAULT 0,
    htf_trend TEXT DEFAULT 'neutral',

    -- Volatility
    atr14 DOUBLE PRECISION DEFAULT 0,
    atr_pct DOUBLE PRECISION DEFAULT 0,
    volatility_regime TEXT DEFAULT 'normal',

    -- Momentum
    roc DOUBLE PRECISION DEFAULT 0,
    rsi DOUBLE PRECISION DEFAULT 50,
    macd DOUBLE PRECISION DEFAULT 0,
    macd_signal DOUBLE PRECISION DEFAULT 0,
    macd_histogram DOUBLE PRECISION DEFAULT 0,

    -- Structure
    swing_high DOUBLE PRECISION,
    swing_low DOUBLE PRECISION,
    order_blocks JSONB DEFAULT '[]',
    fvg_list JSONB DEFAULT '[]',

    -- Liquidity
    volume_ratio DOUBLE PRECISION DEFAULT 1.0,
    volume_ma DOUBLE PRECISION DEFAULT 0,
    liquidity_score DOUBLE PRECISION DEFAULT 50,
    liquidity_zone TEXT DEFAULT 'at_price',

    -- Session
    session TEXT DEFAULT 'london',
    session_quality DOUBLE PRECISION DEFAULT 50,
    asian_range DOUBLE PRECISION DEFAULT 0,
    london_range DOUBLE PRECISION DEFAULT 0,
    ny_range DOUBLE PRECISION DEFAULT 0,

    -- S/R
    support_levels JSONB DEFAULT '[]',
    resistance_levels JSONB DEFAULT '[]',
    nearest_support DOUBLE PRECISION DEFAULT 0,
    nearest_resistance DOUBLE PRECISION DEFAULT 0,

    -- Correlation
    correlations JSONB DEFAULT '{}',
    correlation_significant JSONB DEFAULT '{}',

    -- Cross-asset
    dxy_price DOUBLE PRECISION,
    eurusd_price DOUBLE PRECISION,
    xag_price DOUBLE PRECISION,
    us10y_yield DOUBLE PRECISION,
    spx_price DOUBLE PRECISION,

    -- Regime
    regime regime_type DEFAULT 'ranging',
    regime_confidence DOUBLE PRECISION DEFAULT 0.5,

    -- Meta
    is_outlier BOOLEAN DEFAULT FALSE,
    quality_scores JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_features_time ON feature_sets (symbol, timeframe, timestamp DESC);

-- ============================================================
-- SIGNALS
-- ============================================================
CREATE TABLE signals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    symbol TEXT NOT NULL DEFAULT 'XAUUSD',
    timeframe TEXT NOT NULL DEFAULT '5m',
    direction signal_direction NOT NULL DEFAULT 'neutral',
    confidence DOUBLE PRECISION NOT NULL DEFAULT 0,
    entry_price DOUBLE PRECISION NOT NULL DEFAULT 0,
    stop_loss DOUBLE PRECISION NOT NULL DEFAULT 0,
    tp1 DOUBLE PRECISION NOT NULL DEFAULT 0,
    tp2 DOUBLE PRECISION NOT NULL DEFAULT 0,
    tp1_rr DOUBLE PRECISION DEFAULT 0,
    tp2_rr DOUBLE PRECISION DEFAULT 0,

    -- Computed scores
    bull_score DOUBLE PRECISION DEFAULT 0,
    bear_score DOUBLE PRECISION DEFAULT 0,
    range_score DOUBLE PRECISION DEFAULT 0,
    trend_score DOUBLE PRECISION DEFAULT 0,
    liq_score DOUBLE PRECISION DEFAULT 0,
    session_score DOUBLE PRECISION DEFAULT 0,
    analog_score DOUBLE PRECISION DEFAULT 0,
    confidence_components JSONB DEFAULT '{}',

    -- Market state
    regime regime_type DEFAULT 'ranging',
    feature_set_id UUID REFERENCES feature_sets(id),

    -- Audit
    source_engine TEXT DEFAULT 'strategy_engine',
    ri1_requirements TEXT[] DEFAULT '{}',
    decision_path JSONB DEFAULT '[]',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_signals_time ON signals (symbol, timestamp DESC);

-- ============================================================
-- TRADES
-- ============================================================
CREATE TABLE trades (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    signal_id UUID REFERENCES signals(id),
    symbol TEXT NOT NULL DEFAULT 'XAUUSD',
    direction signal_direction NOT NULL,

    -- Entry
    entry_price DOUBLE PRECISION NOT NULL,
    entry_time TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    entry_signal_time TIMESTAMPTZ,
    entry_bar_time TIMESTAMPTZ,

    -- Stops / Targets
    stop_loss DOUBLE PRECISION NOT NULL,
    tp1 DOUBLE PRECISION NOT NULL,
    tp2 DOUBLE PRECISION NOT NULL,
    tp1_rr DOUBLE PRECISION DEFAULT 0,
    tp2_rr DOUBLE PRECISION DEFAULT 0,

    -- Execution
    size DOUBLE PRECISION NOT NULL DEFAULT 0,
    fill_quality TEXT DEFAULT 'perfect',
    spread_cost DOUBLE PRECISION DEFAULT 0,
    commission DOUBLE PRECISION DEFAULT 0,
    slippage DOUBLE PRECISION DEFAULT 0,

    -- Exit
    exit_price DOUBLE PRECISION,
    exit_time TIMESTAMPTZ,
    exit_reason exit_reason,
    exit_bar_time TIMESTAMPTZ,

    -- P&L
    pnl DOUBLE PRECISION,
    pnl_pct DOUBLE PRECISION,
    rr_realized DOUBLE PRECISION,

    -- Risk
    risk_amount DOUBLE PRECISION DEFAULT 0,
    risk_pct DOUBLE PRECISION DEFAULT 0,

    -- Audit
    execution_timing_ms DOUBLE PRECISION DEFAULT 0,
    decision_path JSONB DEFAULT '[]',
    ri1_requirements TEXT[] DEFAULT '{}',
    status trade_status DEFAULT 'open',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_trades_status ON trades (status, created_at DESC);
CREATE INDEX idx_trades_symbol ON trades (symbol, entry_time DESC);

-- ============================================================
-- AUDIT
-- ============================================================
CREATE TABLE audit_log (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    engine TEXT NOT NULL,
    event_type TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'info',
    score DOUBLE PRECISION,
    details JSONB DEFAULT '{}',
    trace_id UUID NOT NULL DEFAULT uuid_generate_v4(),
    ri1_requirement_ids TEXT[] DEFAULT '{}'
);

CREATE INDEX idx_audit_time ON audit_log (timestamp DESC);
CREATE INDEX idx_audit_trace ON audit_log (trace_id);

CREATE TABLE audit_scores (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    run_timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    overall DOUBLE PRECISION NOT NULL,
    mathematical_correctness DOUBLE PRECISION DEFAULT 0,
    statistical_validity DOUBLE PRECISION DEFAULT 0,
    dashboard_consistency DOUBLE PRECISION DEFAULT 0,
    timing_integrity DOUBLE PRECISION DEFAULT 0,
    risk_management DOUBLE PRECISION DEFAULT 0,
    code_quality DOUBLE PRECISION DEFAULT 0,
    recommendations TEXT[] DEFAULT '{}',
    details JSONB DEFAULT '{}'
);

CREATE INDEX idx_audit_scores_time ON audit_scores (run_timestamp DESC);

-- ============================================================
-- VALIDATION
-- ============================================================
CREATE TABLE validation_runs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    stream validation_stream NOT NULL,
    date_from DATE NOT NULL,
    date_to DATE NOT NULL,
    bars_processed INTEGER NOT NULL DEFAULT 0,
    status TEXT NOT NULL DEFAULT 'running',
    overall_pass_rate DOUBLE PRECISION,
    started_at TIMESTAMPTZ DEFAULT NOW(),
    completed_at TIMESTAMPTZ
);

CREATE TABLE validation_results (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    run_id UUID NOT NULL REFERENCES validation_runs(id) ON DELETE CASCADE,
    check_name TEXT NOT NULL,
    formula_id TEXT,
    input_variables JSONB,
    expected_value DOUBLE PRECISION,
    actual_value DOUBLE PRECISION,
    difference DOUBLE PRECISION,
    tolerance DOUBLE PRECISION DEFAULT 1e-6,
    passed BOOLEAN NOT NULL,
    details TEXT
);

CREATE INDEX idx_val_results_run ON validation_results (run_id);

-- ============================================================
-- REQUIREMENTS (RTM)
-- ============================================================
CREATE TABLE requirements (
    id TEXT PRIMARY KEY,
    origin TEXT NOT NULL,
    description TEXT NOT NULL,
    status requirement_status DEFAULT 'draft',
    backend_module TEXT,
    test_status TEXT DEFAULT 'pending',
    notes TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE requirement_dependencies (
    requirement_id TEXT REFERENCES requirements(id),
    depends_on_id TEXT REFERENCES requirements(id),
    PRIMARY KEY (requirement_id, depends_on_id)
);

-- ============================================================
-- PROJECT INTELLIGENCE (PIE)
-- ============================================================
CREATE TABLE pie_todos (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    content TEXT NOT NULL,
    status todo_status DEFAULT 'pending',
    priority TEXT DEFAULT 'medium',
    owner TEXT,
    requirement_ids TEXT[] DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    completed_at TIMESTAMPTZ
);

CREATE TABLE pie_releases (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    version TEXT NOT NULL UNIQUE,
    release_date DATE NOT NULL DEFAULT CURRENT_DATE,
    requirement_ids TEXT[] DEFAULT '{}',
    notes TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================
-- RISK
-- ============================================================
CREATE TABLE risk_limits (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    limit_type TEXT NOT NULL UNIQUE,
    limit_value DOUBLE PRECISION NOT NULL,
    current_value DOUBLE PRECISION DEFAULT 0,
    breached BOOLEAN DEFAULT FALSE,
    breached_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE risk_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_type TEXT NOT NULL,
    severity TEXT NOT NULL,
    description TEXT NOT NULL,
    details JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================
-- FORMULA REGISTRY (PIE)
-- ============================================================
CREATE TABLE formula_registry (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    expression TEXT NOT NULL,
    source TEXT,
    validation_status TEXT DEFAULT 'unverified',
    dependencies TEXT[] DEFAULT '{}',
    version INTEGER DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================
-- DASHBOARD SNAPSHOTS
-- ============================================================
CREATE TABLE dashboard_snapshots (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    timestamp TIMESTAMPTZ NOT NULL,
    symbol TEXT NOT NULL,
    timeframe TEXT NOT NULL,
    data JSONB NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_dash_time ON dashboard_snapshots (symbol, timestamp DESC);

-- ============================================================
-- INITIAL DATA
-- ============================================================
INSERT INTO risk_limits (limit_type, limit_value) VALUES
    ('daily_loss', -500),
    ('weekly_loss', -1500),
    ('max_drawdown', -0.15),
    ('portfolio_heat', 0.50);
