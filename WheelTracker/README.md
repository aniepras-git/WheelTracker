# WheelTracker: GROK-Powered Options Wheel Strategy Dashboard

A C# WPF app for tracking wheel strategy trades: log positions, monitor live prices (via Yahoo Finance/Alpha Vantage), calculate returns/ITM alerts, and visualize P&L. Built collaboratively with Grok (xAI) for clean MVVM code, real-time updates, and Excel-like views.

## Quick Start
- Clone: `git clone https://github.com/aniepras-git/WheelTracker.git`
- Open `WheelTracker.sln` in Visual Studio 2022.
- Install NuGets: YahooFinanceApi, LiveChartsCore.SkiaSharpView.WPF, CommunityToolkit.Mvvm.
- Run: Add your API key in `App.xaml.cs`, hit F5 for the dashboard.

## Features
- **Trade Entry**: Pop-up for new options (strikes, premiums, DTE calcs).
- **Open Positions**: Grid with filtering, close types (EXP/Assign), annual returns.
- **Live Data**: Auto-refresh prices, green/red indicators.
- **DB**: SQLite for trades (Entity Framework Core).

## GROK Collaboration
This project stems from Grok chat sessions—features like PriceService and converters added iteratively. For updates/debugging, share commit links here: https://github.com/aniepras-git/WheelTracker/commits/main.

## Tech Stack
- .NET 8 WPF
- MVVM Toolkit
- SQLite + EF Core
- LiveCharts for visuals

Questions? Ping in the Grok chat—next up: ITM alerts or portfolio reports?