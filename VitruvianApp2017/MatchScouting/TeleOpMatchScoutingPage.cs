using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;
using Xamarin.Forms;

namespace VitruvianApp2017 {
	public class TeleOpMatchScoutingPage : ContentPage {
		Grid pageLayout = new Grid() {
			HorizontalOptions = LayoutOptions.FillAndExpand,
			VerticalOptions = LayoutOptions.FillAndExpand,
			BackgroundColor = Color.White,

			RowDefinitions = {
				/*
				new RowDefinition() { Height = GridLength.Auto},
				new RowDefinition() { Height = GridLength.Auto},
				new RowDefinition() { Height = GridLength.Auto},
				new RowDefinition() { Height = GridLength.Auto},
				new RowDefinition() { Height = GridLength.Auto},
				new RowDefinition() { Height = GridLength.Auto},
				new RowDefinition() { Height = GridLength.Star},
				new RowDefinition() { Height = GridLength.Auto},
				*/
			},
			ColumnDefinitions = {
			}
		};
		ColorButton[] inputs = new ColorButton[3];
		ColorButton gearScoredBtn, climbSuccessBtn, climbAttemptBtn;
		Button cycleUndo, lowGoalDumpBtn, clearInputsBtn;
		SingleCounter cycleCounter, gearsStationDropped, gearsTransitDropped;
		ColorButtonStackArray hopperCapacity, goalAccuracy;
		bool lowDumpOn = false;
		Label teleOpPressureLbl, teleOpGearLbl, actionCounter;
		int teleOpGears = 0, teleOpPressure = 0, cPressure = 0;
		int robotMaxCapacity = 0;
		ActionData[] mActions = new ActionData[999];
		StackLayout lastActionView;
		int aIndex = 0, aCount = 0;
		Label[] lastActionLabels = new Label[7];
		ContentView emptyWidth;
		MatchData matchData;
		int mType;

		public TeleOpMatchScoutingPage(MatchData data, int matchType) {
			mType = matchType;
			matchData = data;
			getMaxCapacity();

			Title = "TeleOp Mode";

			Label teamNumberLbl = new Label() {
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Text = "Team: " + matchData.teamNumber.ToString(),
				TextColor = Color.White,
				BackgroundColor = Color.Green,
				FontSize = GlobalVariables.sizeSmall,
				FontAttributes = FontAttributes.Bold
			};

			teleOpPressureLbl = new Label() {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Text = "Pressure: " + (matchData.autoPressure + teleOpPressure),
				TextColor = Color.White,
				BackgroundColor = Color.Green,
				FontSize = GlobalVariables.sizeSmall,
				FontAttributes = FontAttributes.Bold
			};

			teleOpGearLbl = new Label() {
				HorizontalOptions = LayoutOptions.EndAndExpand,
				Text = "Gears: " + ((matchData.autoGearScored ? 1 : 0) + teleOpGears),
				TextColor = Color.White,
				BackgroundColor = Color.Green,
				FontSize = GlobalVariables.sizeSmall,
				FontAttributes = FontAttributes.Bold
			};

			Label robotFuelCapacityLbl = new Label() {
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Text = "Max Fuel Capacity: " + robotMaxCapacity,
				TextColor = Color.White,
				FontSize = GlobalVariables.sizeTiny,
				FontAttributes = FontAttributes.Bold,
			};

			Label actionCountLbl = new Label() {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Text = "Actions:",
				TextColor = Color.Black,
				FontSize = GlobalVariables.sizeMedium,
				FontAttributes = FontAttributes.Bold
			};

			actionCounter = new Label() {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Text = "0",
				TextColor = Color.Black,
				FontSize = GlobalVariables.sizeTitle,
				FontAttributes = FontAttributes.Bold
			};

			for (int i = 0; i < 7; i++)
				lastActionLabels[i] = new Label() {
					HorizontalOptions = LayoutOptions.StartAndExpand,
					TextColor = Color.Black,
					FontSize = GlobalVariables.sizeSmall,
					FontAttributes = FontAttributes.Bold 
				};

			cycleUndo = new Button() {
				Text = "Undo Action",
				BackgroundColor = Color.Yellow,
				FontAttributes = FontAttributes.Bold,
				FontSize = GlobalVariables.sizeMedium
			};
			cycleUndo.Clicked += (sender, e) => {
				if (aIndex > 0) {
					undoAction();
					cycleUndo.BackgroundColor = Color.Orange;
				}
			};
			clearInputsBtn = new Button() {
				Text = "Clear Inputs",
				BackgroundColor = Color.Red,
				FontAttributes = FontAttributes.Bold,
				FontSize = GlobalVariables.sizeMedium,
			};
			clearInputsBtn.Clicked += (sender, e) => {
				clearValues();
			};

			var fuelLbl = new Label() {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Text = "Fuel",
				TextColor = Color.Black,
				FontSize = GlobalVariables.sizeMedium,
				FontAttributes = FontAttributes.Bold
			};
			hopperCapacity = new ColorButtonStackArray("Capacity", 5);
			goalAccuracy = new ColorButtonStackArray("Accuracy", 5);
			emptyWidth = new ContentView() {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				IsVisible = false
			};
			lowGoalDumpBtn = new Button() {
				Text = "Low Goal Dump",
				BackgroundColor = Color.Green,
				FontAttributes = FontAttributes.Bold,
				FontSize = GlobalVariables.sizeSmall
			};
			lowGoalDumpBtn.Clicked += (sender, e) => {
				addAction(2);
			};

			var highScoreBtn = new Button() {
				Text = "High Goal Shoot",
				BackgroundColor = Color.Green,
				FontSize = GlobalVariables.sizeSmall,
				FontAttributes = FontAttributes.Bold
			};
			highScoreBtn.Clicked += (sender, e) => {
				addAction(1);
			};

			gearScoredBtn = new ColorButton("Gear Scored");
			gearScoredBtn.Clicked += (sender, e) => {
				addAction(0);
			};

			var droppedLbl = new Label() {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Text = "Gears Dropped:",
				TextColor = Color.Black,
				FontSize = GlobalVariables.sizeMedium,
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center
			};
			gearsStationDropped = new SingleCounter("At Station");
			gearsTransitDropped = new SingleCounter("In Transit");

			var finishBtn = new Button() {
				Text = "FINISH",
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				FontSize = GlobalVariables.sizeMedium,
				FontAttributes = FontAttributes.Bold,
				BackgroundColor = Color.Yellow
			};

			finishBtn.Clicked += (sender, e) => {
				addAction(3);
				saveData();
				Navigation.PushModalAsync(new PostMatchScoutingPage(matchData, mType));
			};


			var topBar = new Grid() {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.Green,
				RowSpacing = 0,
				ColumnSpacing = 0,

				RowDefinitions = {
				}
			};

			lastActionView = new StackLayout() {
				BackgroundColor = Color.Silver
			};
			foreach (var lbl in lastActionLabels)
				lastActionView.Children.Add(lbl);

			topBar.Children.Add(teamNumberLbl, 0, 0);
			topBar.Children.Add(robotFuelCapacityLbl, 0, 1);
			topBar.Children.Add(teleOpGearLbl, 2, 0);
			topBar.Children.Add(teleOpPressureLbl, 1, 0);

			pageLayout.Children.Add(actionCountLbl, 0, 0);
			pageLayout.Children.Add(actionCounter, 0, 1);
			pageLayout.Children.Add(cycleUndo, 0, 2);
			pageLayout.Children.Add(lastActionView, 0, 1, 3, 8);

			pageLayout.Children.Add(fuelLbl, 1, 3, 0, 1);
			pageLayout.Children.Add(hopperCapacity, 1, 2, 1, 6);
			pageLayout.Children.Add(goalAccuracy, 2, 3, 1, 6);
			//pageLayout.Children.Add(emptyWidth, 2, 3, 8, 9);
			pageLayout.Children.Add(lowGoalDumpBtn, 2, 3, 6, 7);
			pageLayout.Children.Add(highScoreBtn, 2, 3, 7, 8);

			pageLayout.Children.Add(gearScoredBtn, 3, 4, 0, 1);
			pageLayout.Children.Add(droppedLbl, 3, 4, 1, 2);
			pageLayout.Children.Add(gearsStationDropped, 3, 4, 2, 4);
			pageLayout.Children.Add(gearsTransitDropped, 3, 4, 4, 6);
			pageLayout.Children.Add(clearInputsBtn, 3, 4, 6, 7);
			pageLayout.Children.Add(cycleUndo, 3, 4, 7, 8);


			pageLayout.Children.Add(finishBtn, 1, 3, 9, 10);

			Content = new StackLayout() {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Spacing = 0,

				Children = {
					topBar,
					new ScrollView(){
						IsClippedToBounds = true,

						Content = pageLayout
					}
				}
			};
		}


		protected override void OnAppearing() {
			base.OnAppearing();

			foreach (var btn in hopperCapacity.btnArray)
				btn.WidthRequest = lowGoalDumpBtn.Width;

			foreach (var btn in goalAccuracy.btnArray)
				btn.WidthRequest = lowGoalDumpBtn.Width;
		}


		void addAction(int v) {
			/*
			bool t = false;
			mActions[aIndex] = new ActionData();
			try {
				foreach (var hopper in hopperCapacity.on)
					if (hopper == true) {
						t = true;
						break;
					}
				if (t)
					mActions[aIndex].hopperCapacity = hopperCapacity.getAvgPercentage() * robotMaxCapacity;
				else
					mActions[aIndex].hopperCapacity = 0;
			}
			catch{
				mActions[aIndex].hopperCapacity = 0;
			}

			if (v == 0) // gear scored
				mActions[aIndex].cyclePressure = 0;
			else if (v == 1) // high shots
				mActions[aIndex].cyclePressure = (int)Math.Floor((robotMaxCapacity * hopperCapacity.getAvgPercentage() * goalAccuracy.getAvgPercentage()) / 3);
			else if (v == 2) { // low shots
				mActions[aIndex].lowGoalDump = true;
				mActions[aIndex].cyclePressure = (int)Math.Floor((robotMaxCapacity * hopperCapacity.getAvgPercentage()) / 9);
			} else // no complete scoring action
				mActions[aIndex].lowGoalDump = false;

			mActions[aIndex].gearsStationDrop = gearsStationDropped.getValue();
			mActions[aIndex].gearsTransitDrop = gearsTransitDropped.getValue();
			mActions[aIndex].gearSet = gearScoredBtn.on;

			if (gearScoredBtn.on) {
				gearScoredBtn.on = false;
				gearScoredBtn.BackgroundColor = Color.Red;
			}

			lastActionLabels[0].Text = "Hopper Capacity: " + mActions[aIndex].hopperCapacity;
			lastActionLabels[1].Text = "High Accuracy: " + mActions[aIndex].highGoalAccuracy;
			lastActionLabels[2].Text = "Low Goal Dump: " + mActions[aIndex].lowGoalDump;
			lastActionLabels[3].Text = "Cycle Pressure: " + mActions[aIndex].cyclePressure;
			lastActionLabels[4].Text = "Gear Set: " + mActions[aIndex].gearSet;
			lastActionLabels[5].Text = "Gear Station Drops: " + mActions[aIndex].gearsStationDrop;
			lastActionLabels[6].Text = "Gear Transit Drops: " + mActions[aIndex].gearsTransitDrop;
			*/
			clearValues();

			if (v != 3) // aCount != aIndex in the event that the last 'action' the robot performs yeilds no score
				aCount++;
			aIndex++;
			actionCounter.Text = aIndex.ToString();
			actionCounter.BackgroundColor = Color.Transparent;
			cycleUndo.BackgroundColor = Color.Yellow;
		}

		void undoAction() {
			if (aIndex > 1) {
				/*
				lastActionLabels[0].Text = "Hopper Capacity: " + mActions[aIndex - 1].hopperCapacity;
				lastActionLabels[1].Text = "High Accuracy: " + mActions[aIndex - 1].highGoalAccuracy;
				lastActionLabels[2].Text = "Low Goal Dump: " + mActions[aIndex - 1].lowGoalDump;
				lastActionLabels[3].Text = "Cycle Pressure: " + mActions[aIndex - 1].cyclePressure;
				lastActionLabels[4].Text = "Gear Set: " + mActions[aIndex - 1].gearSet;
				lastActionLabels[5].Text = "Gear Station Drops: " + mActions[aIndex - 1].gearsStationDrop;
				lastActionLabels[6].Text = "Gear Transit Drops: " + mActions[aIndex - 1].gearsTransitDrop;
				aIndex--;
				*/
			} else {
				lastActionLabels[0].Text = "Hopper Capacity: " + 0;
				lastActionLabels[1].Text = "High Accuracy: " + 0;
				lastActionLabels[2].Text = "Low Goal Dump: " + false;
				lastActionLabels[3].Text = "Cycle Pressure: " + 0;
				lastActionLabels[4].Text = "Gear Set: " + false;
				lastActionLabels[5].Text = "Gear Station Drops: " + 0;
				lastActionLabels[6].Text = "Gear Transit Drops: " + 0;
			}

			actionCounter.Text = aCount.ToString();
			actionCounter.BackgroundColor = Color.Orange;
			lastActionView.BackgroundColor = Color.Orange;
		}

		void clearValues() {
			for (int i = 0; i < 5; i++) {
				hopperCapacity.on[i] = false;
				hopperCapacity.btnArray[i].BackgroundColor = Color.Red;
				goalAccuracy.on[i] = false;
				goalAccuracy.btnArray[i].BackgroundColor = Color.Red;
			}
			lowDumpOn = false;

			gearsStationDropped.setValue(0);
			gearsTransitDropped.setValue(0);

		}

		void calcObjects() {
			teleOpGears = matchData.autoGearScored ? 1 : 0;
			teleOpPressure = 0;

			for (int i = 0; i < aIndex; i++) {
				/*
				teleOpGears += mActions[i].gearSet ? 1 : 0;
				teleOpPressure += mActions[i].cyclePressure;
				*/
			}
			if (lowDumpOn)
				cPressure += (int)Math.Floor(hopperCapacity.getAvgPercentage() * robotMaxCapacity);
			else
				cPressure += (int)Math.Floor((hopperCapacity.getAvgPercentage() * robotMaxCapacity * goalAccuracy.getAvgPercentage()) / 3);

			teleOpPressureLbl.Text = "Pressure: " + teleOpPressure + cPressure + matchData.autoPressure;
			teleOpGearLbl.Text = "Gears: " + ((matchData.autoGearScored ? 1 : 0) + teleOpGears);
		}

		async Task getMaxCapacity() {
			try {
				var db = new FirebaseClient(GlobalVariables.firebaseURL);
				bool semaphore = true;

				var teamData = db
						.Child(GlobalVariables.regionalPointer)
						.Child("teamData")
						.Child(matchData.teamNumber.ToString())
						.OnceSingleAsync<TeamData>()
						.ContinueWith((arg) => {
							robotMaxCapacity = arg.Result.maxFuelCapacity;
							semaphore = false;
						});
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				while (semaphore) {
					if (stopwatch.ElapsedMilliseconds > 1000) {
						stopwatch.Stop();
						throw new TimeoutException();
					}
				}
			} catch (Exception ex) {
				Console.WriteLine("getMaxCapacity error: " + ex.Message);
				robotMaxCapacity = 0;
			}
		}

		async Task saveData() {
			if (CheckInternetConnectivity.InternetStatus()) {
				calcObjects();
				/*
				matchData.actionCount = aCount;
				matchData.teleOpTotalPressure = teleOpPressure;
				matchData.teleOpGearsDeposit = teleOpGears;

				int stationDropped = 0, transitDropped = 0;
				double hAcc = 0;
				for (int i = 0; i < aIndex; i++) {
					hAcc += mActions[i].highGoalAccuracy;
					stationDropped += mActions[i].gearsStationDrop;
					transitDropped += mActions[i].gearsTransitDrop;
				}
				matchData.teleOpHighAcc = hAcc / aCount;
				matchData.teleOpGearsStationDropped = stationDropped;
				matchData.teleOpGearsTransitDropped = transitDropped;
				*/

				var db = new FirebaseClient(GlobalVariables.firebaseURL);
				string path = "ERROR";

				if (mType == -1)
					path = "practiceMatchData/" + matchData.matchID;
				else {
					path = "matchData/" + matchData.matchID;
					FirebaseAccess.saveData(db, "matchActionData/" + matchData.matchID, mActions);
				}

				FirebaseAccess.saveData(db, path, matchData);

				/*
				if (mType == -1) {
					var send = db
								.Child(GlobalVariables.regionalPointer)
								.Child("PracticeMatches")
								.Child(matchData.teamNumber.ToString())
								.Child(matchData.matchNumber.ToString())
								.PutAsync(matchData);
				} else {
					var matchBreakDown = db
										.Child(GlobalVariables.regionalPointer)
										.Child("teamMatchActionData")
										.Child(matchData.teamNumber.ToString())
										.Child(matchData.matchNumber.ToString())
										.PutAsync(mActions);

					var fbTeam = db
								.Child(GlobalVariables.regionalPointer)
								.Child("teamMatchData")
								.Child(matchData.teamNumber.ToString())
								.Child(matchData.matchNumber.ToString())
								.PutAsync(matchData);
				}
				*/
			}
		}
	}
}
