﻿using BackendAPI.Objects;
using BackendAPI.Objects.Implementation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace BackendAPI.DeckText
{
    public class TextLabel
    {

        private string text;
        private Brush brush;
        private int position;
        private int size;
        private Color color;


        public void setPosition(int position)
        {
     int pos = 0;
            switch (position)
            {
                case 81:
                    pos = 50;
                    break;
                case 17:
                    pos = 25;
                    break;
                case 49:
                    pos = 1;
                    break;
            }
            this.position = pos;

        }
        public TextLabel(DeckItemMisc item)
        {
            if (item != null)
            {
                setPosition(item.Deckposition);
                this.size = item.Decksize / 3;
                this.Color = System.Drawing.ColorTranslator.FromHtml(item.Deckcolor);
                this.text = item.Deckname;
                this.brush = new SolidBrush(this.Color); //Brushes.White;

            }
        }

        public string Text { get => text; set => text = value; }
        public Brush Brush { get => brush; set => brush = value; }
   
        public int Position { get => position; set => position = value; }
        public int Size { get => size; set => size = value; }
        public Color Color { get => color; set => color = value; }
    }
}
