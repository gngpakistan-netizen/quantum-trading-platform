# Terraform — Cloudflare Infrastructure
# Apply with: terraform apply -var="cloudflare_api_token=..."

variable "cloudflare_api_token" {
  type        = string
  sensitive   = true
  description = "Cloudflare API Token"
}

variable "account_id" {
  type        = string
  description = "Cloudflare Account ID"
}

variable "zone_id" {
  type        = string
  description = "Cloudflare Zone ID for custom domain"
}

variable "domain" {
  type        = string
  description = "Custom domain (e.g. quantum.yourdomain.com)"
}

terraform {
  required_providers {
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "~> 4.0"
    }
  }
}

provider "cloudflare" {
  api_token = var.cloudflare_api_token
}

# D1 Database
resource "cloudflare_d1_database" "quantum_db" {
  account_id = var.account_id
  name       = "xauusd-quantum-db"
}

# R2 Bucket
resource "cloudflare_r2_bucket" "market_data" {
  account_id = var.account_id
  name       = "xauusd-market-data"
  location   = "WEUR"
}

# KV Namespace
resource "cloudflare_workers_kv_namespace" "realtime_state" {
  account_id = var.account_id
  title      = "xauusd-quantum-realtime"
}

# Workers API
resource "cloudflare_workers_script" "quantum_api" {
  account_id = var.account_id
  name       = "xauusd-quantum-api"
  content    = filebase64sha256("../../apps/api/dist/index.js")
  module     = true

  d1_database_bindings {
    name        = "DB"
    database_id = cloudflare_d1_database.quantum_db.id
  }

  r2_bucket_bindings {
    name        = "MARKET_DATA"
    bucket_name = cloudflare_r2_bucket.market_data.name
  }

  kv_namespace_bindings {
    name         = "REALTIME_STATE"
    namespace_id = cloudflare_workers_kv_namespace.realtime_state.id
  }
}

# Pages Project
resource "cloudflare_pages_project" "quantum_frontend" {
  account_id        = var.account_id
  name              = "xauusd-quantum"
  production_branch = "main"

  build_config {
    build_command   = "cd apps/web && pnpm build"
    destination_dir = "apps/web/out"
    root_dir        = ""
  }

  source {
    type = "github"
    config {
      owner             = "your-github-username"
      repo_name         = "xauusd-quantum-terminal"
      production_branch = "main"
    }
  }
}

# Custom Domain Route
resource "cloudflare_pages_domain" "quantum_domain" {
  account_id   = var.account_id
  project_name = cloudflare_pages_project.quantum_frontend.name
  domain       = var.domain
}
