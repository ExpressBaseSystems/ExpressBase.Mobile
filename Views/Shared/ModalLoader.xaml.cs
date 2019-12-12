using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ModalLoader : ContentView
	{
		public ModalLoader ()
		{
			InitializeComponent ();
		}
	}
}