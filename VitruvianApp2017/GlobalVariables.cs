﻿using System;
using Xamarin.Forms;

namespace VitruvianApp2017
{
	public static class GlobalVariables
	{
		// Modifiable per competition/year
		public static string regionalPointer = "2017calb";
		public static string firebaseApplicationID = "1:222280551868:android:051a820459657e45\n";
		public static string firebaseAPIKey = "AIzaSyDeNEa_Dgr7AfDfUyBWX6D5Le80H9t26es";
		public static string firebaseURL = "https://vitruvianapptest.firebaseio.com/";
		public static string firebaseStorageURL = "gs://vitruvianapptest.appspot.com/";

		// Don't modify these
		public static double sizeTitle = Device.GetNamedSize(NamedSize.Large, typeof(Label));
		public static double sizeMedium = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
		public static double sizeSmall = Device.GetNamedSize(NamedSize.Small, typeof(Label));

		//public static string [] parseStrings = { };
	}
}
