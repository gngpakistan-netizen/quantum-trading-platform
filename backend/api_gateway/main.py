from fastapi import FastAPI

app = FastAPI(title="XAUUSD Quantum Platform API")

@app.get("/")
def root():
    return {"status": "Quantum Platform is running", "version": "4.0"}

@app.post("/webhook")
async def tradingview_webhook(payload: dict):
    # TODO: Process TradingView alert
    print("Received alert:", payload)
    return {"status": "received"}
