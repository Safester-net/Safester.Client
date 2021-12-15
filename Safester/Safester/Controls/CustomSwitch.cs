using System;
using Xamarin.Forms;

namespace Safester.Controls
{
    public class CustomSwitch : Switch
    {
        public Action ColorChangedEvent { get; set; }
    }
}
