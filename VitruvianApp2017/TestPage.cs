﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;

namespace VitruvianApp2017
{
	public class TestPage:ContentPage
	{
		ScrollView teamIndex;
		StackLayout teamStack = new StackLayout()
		{
			Spacing = 1,
			BackgroundColor = Color.Silver
		};

		ActivityIndicator busyIcon = new ActivityIndicator();

		public TestPage()
		{
		}
	}
}