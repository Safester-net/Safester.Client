using System;
using Xamarin.Forms;

namespace Safester.Controls
{
    public class CustomEditor : Editor
    {
        public Action SetStartPosition { get; set; }

        public CustomEditor()
        {
        }
    }
}
