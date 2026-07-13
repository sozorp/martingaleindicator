#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Martingaleindicator
{
	public class LuisMartingala : Indicator
	{
		private Gui.Chart.Chart					chartWindow;
		private System.Windows.Controls.Grid		chartTraderGrid;
		private System.Windows.Controls.Grid		labelGrid;
		private System.Windows.Controls.TextBlock	labelTextBlock;
		private System.Windows.Controls.RowDefinition addedRow;
		private bool								panelActive;
		private int									chartTraderBaseRowCount;
		private Position							position;

		private const string						StopLossLineTag = "LuisMartingalaStopLossLine";

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name = "Riesgo en USD", Order = 1, GroupName = "Parameters")]
		public double RiskUsd { get; set; }

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "LuisMartingala";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event.
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

				RiskUsd										= 200;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				foreach (Account a in Account.All)
				{
					a.PositionUpdate += OnPositionUpdate;

					Position existing = a.Positions.FirstOrDefault(p => p.Instrument == Instrument && p.MarketPosition != MarketPosition.Flat);
					if (existing != null)
						position = existing;
				}
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						CreateChartTraderLabel();
					});
				}
			}
			else if (State == State.Terminated)
			{
				foreach (Account a in Account.All)
					a.PositionUpdate -= OnPositionUpdate;

				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						RemoveChartTraderLabel();
					});
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 0)
				return;

			UpdateStopLossLine();
		}

		private void OnPositionUpdate(object sender, PositionEventArgs e)
		{
			if (e.Position.Instrument != Instrument)
				return;

			position = e.Position;

			if (ChartControl != null)
				ChartControl.Dispatcher.InvokeAsync(() => UpdateStopLossLine());
			else
				UpdateStopLossLine();
		}

		private void UpdateStopLossLine()
		{
			if (position == null || position.MarketPosition == MarketPosition.Flat || position.Quantity == 0)
			{
				RemoveDrawObject(StopLossLineTag);
				return;
			}

			double pointValue		= Instrument.MasterInstrument.PointValue;
			double avgPrice		= position.AveragePrice;
			int quantity			= position.Quantity;

			double riskPerContract	= RiskUsd / quantity;
			double priceOffset		= riskPerContract / pointValue;

			double stopPrice = position.MarketPosition == MarketPosition.Long
				? avgPrice - priceOffset
				: avgPrice + priceOffset;

			stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);

			Draw.HorizontalLine(this, StopLossLineTag, stopPrice, Brushes.OrangeRed, DashStyleHelper.Dash, 2);
		}

		#region Chart Trader Label

		private void CreateChartTraderLabel()
		{
			chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;

			// if not added to a chart, do nothing
			if (chartWindow == null)
				return;

			chartTraderGrid = (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;

			if (chartTraderGrid == null)
				return;

			labelGrid = new System.Windows.Controls.Grid();
			labelGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

			addedRow = new System.Windows.Controls.RowDefinition() { Height = GridLength.Auto };

			labelTextBlock = new System.Windows.Controls.TextBlock()
			{
				Text				= string.Format("{0} - {1:yyyy-MM-dd}", Name, DateTime.Now),
				Height				= 25,
				Margin				= new Thickness(5, 2, 5, 2),
				HorizontalAlignment	= HorizontalAlignment.Center,
				VerticalAlignment	= VerticalAlignment.Center,
				Foreground			= Brushes.White,
			};

			System.Windows.Controls.Grid.SetColumn(labelTextBlock, 0);
			System.Windows.Controls.Grid.SetRow(labelTextBlock, 0);
			labelGrid.Children.Add(labelTextBlock);

			chartTraderBaseRowCount = chartTraderGrid.RowDefinitions.Count;

			InsertChartTraderLabel();
		}

		private void InsertChartTraderLabel()
		{
			if (panelActive || chartTraderGrid == null)
				return;

			chartTraderGrid.RowDefinitions.Add(addedRow);
			System.Windows.Controls.Grid.SetRow(labelGrid, chartTraderBaseRowCount);
			chartTraderGrid.Children.Add(labelGrid);

			panelActive = true;
		}

		private void RemoveChartTraderLabel()
		{
			if (!panelActive || chartTraderGrid == null)
				return;

			chartTraderGrid.Children.Remove(labelGrid);
			chartTraderGrid.RowDefinitions.Remove(addedRow);

			panelActive = false;
		}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Martingaleindicator.LuisMartingala[] cacheLuisMartingala;
		public Martingaleindicator.LuisMartingala LuisMartingala(double riskUsd)
		{
			return LuisMartingala(Input, riskUsd);
		}

		public Martingaleindicator.LuisMartingala LuisMartingala(ISeries<double> input, double riskUsd)
		{
			if (cacheLuisMartingala != null)
				for (int idx = 0; idx < cacheLuisMartingala.Length; idx++)
					if (cacheLuisMartingala[idx] != null && cacheLuisMartingala[idx].RiskUsd == riskUsd && cacheLuisMartingala[idx].EqualsInput(input))
						return cacheLuisMartingala[idx];
			return CacheIndicator<Martingaleindicator.LuisMartingala>(new Martingaleindicator.LuisMartingala(){ RiskUsd = riskUsd }, input, ref cacheLuisMartingala);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Martingaleindicator.LuisMartingala LuisMartingala(double riskUsd)
		{
			return indicator.LuisMartingala(Input, riskUsd);
		}

		public Indicators.Martingaleindicator.LuisMartingala LuisMartingala(ISeries<double> input , double riskUsd)
		{
			return indicator.LuisMartingala(input, riskUsd);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Martingaleindicator.LuisMartingala LuisMartingala(double riskUsd)
		{
			return indicator.LuisMartingala(Input, riskUsd);
		}

		public Indicators.Martingaleindicator.LuisMartingala LuisMartingala(ISeries<double> input , double riskUsd)
		{
			return indicator.LuisMartingala(input, riskUsd);
		}
	}
}

#endregion
