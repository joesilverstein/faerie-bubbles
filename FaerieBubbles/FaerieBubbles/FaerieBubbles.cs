using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FaerieBubbles
{
    public partial class FaerieBubbles : Form
    {
        public FaerieBubbles()
        {
            InitializeComponent();
        }

        private Bitmap myCanvas;
        private double theta = 0;
        private int numCanvasChanges=0;
        private int level = 1;
        private int tick = 0;
        private int points = 100;
        Random rand = new Random();

        private int bubblesInExistance = 0;
        private float totalHeight = 0; 
        private List<Bubbles> bubbleList;
        private SizeF bubbleSize;
        private Color orange = Color.Orange;
        private Color blue = Color.DeepSkyBlue;
        private Color green = Color.YellowGreen;
        private Color purple = Color.Purple;
        private Color yellow = Color.Yellow;
        private List<Color> colorsThisLevel; 

        private Queue<Bubbles> nextBubbles;
        private bool launchBubble = false;
        private bool bubbleFlying = false;
        private Bubbles flyingBubble;
        private double flyingTheta;
      

        private void Form1_Load(object sender, EventArgs e)
        {
            myCanvas = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height -25,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bubbleSize = new SizeF(50, 50);
            Graphics g = Graphics.FromImage(myCanvas);
            g.Clear(Color.AliceBlue);
            loadLevel(level);
             
            
            animationTimer.Start();
        }

        private void FaerieBubbles_Resize(object sender, EventArgs e)
        {
            myCanvas = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height - 25,
               System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(myCanvas);
            g.Clear(Color.AliceBlue);

        }
        

        private void FaerieBubbles_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImageUnscaled(myCanvas, 0, 25 + ((this.ClientRectangle.Height - 25)/10)*numCanvasChanges );
        }
        
        private void animationTimer_Tick(object sender, EventArgs e)
        {
            if (tick != 0 && tick % ((60 / (level*2)) * 1000 / animationTimer.Interval) == 0)
            {
                myCanvas = new Bitmap(myCanvas, myCanvas.Width, myCanvas.Height- (this.ClientRectangle.Height - 25)/10);
                for (int i = 0; i < nextBubbles.Count(); i++)
                    nextBubbles.ElementAt(i).setRectangleY(myCanvas.Height - bubbleSize.Height, bubbleSize);
                numCanvasChanges++;
                if (myCanvas.Height < totalHeight + bubbleSize.Height)
                {
                    MessageBox.Show("You loose! Your score is: " + points.ToString() + ". And you reached level: " + level.ToString());
                    animationTimer.Stop();
                }

            }
            Pen launchLinePen = new Pen(Color.Black);
            launchLinePen.Width = 6;
            PointF p1 = new PointF(myCanvas.Width / 2, myCanvas.Height);
            PointF p2 = new PointF((float)(myCanvas.Width / 2 + ClientRectangle.Height/10 *Math.Sin(theta)), (float)(myCanvas.Height - ClientRectangle.Height/10*Math.Cos(theta)));
            Graphics g = Graphics.FromImage(myCanvas);

            if (launchBubble)
            {
                flyingTheta = theta;
                flyingBubble = new Bubbles();
                flyingBubble = nextBubbles.Peek(); 
                bubbleList.Add(nextBubbles.Dequeue());
                for (int i = 0; i < nextBubbles.Count(); i++)
                    nextBubbles.ElementAt(i).changeRectangleX(-bubbleSize.Width,bubbleSize);
                PointF p = new PointF(myCanvas.Width / 2 + bubbleSize.Width/2 + bubbleSize.Width * (nextBubbles.Count()-1), myCanvas.Height - bubbleSize.Height);
                int c = rand.Next(0,colorsThisLevel.Count());
                Color nextColor = colorsThisLevel[c];
                nextBubbles.Enqueue(new Bubbles(nextColor, p, bubbleSize));
                launchBubble = false;
                bubbleFlying = true; 
            }
            if (bubbleFlying)
            {
                PointF FBpoint = bubbleList.ElementAt((bubbleList.Count) - 1).getRectangle().Location;
                if (FBpoint.X + (float)(ClientRectangle.Width / 30 * Math.Sin(flyingTheta)) < this.ClientRectangle.Width-50 && FBpoint.X + (float)(ClientRectangle.Width / 30 * Math.Sin(flyingTheta)) > 0)
                    FBpoint.X += (float)(ClientRectangle.Width / 30 * Math.Sin(flyingTheta));
                else
                    flyingTheta = 2 * Math.PI - flyingTheta;
                if (FBpoint.Y - (float)(ClientRectangle.Height / 30 * Math.Cos(flyingTheta)) > 0)
                    FBpoint.Y -= (float)(ClientRectangle.Height / 30 * Math.Cos(flyingTheta));
                else
                {
                    bubbleFlying = false;
                    FBpoint.Y = 0;
                }
                

                bubbleList.ElementAt(bubbleList.Count() - 1).setloc(FBpoint, bubbleSize);
                bool matchFound = false;

                for (int j = 0; j < bubbleList.Count()-1 && !matchFound; j++)
                {
                    if (bubbleList[bubbleList.Count() - 1].getRectangle().Y - bubbleList[j].getRectangle().Y <= bubbleSize.Height
                        && bubbleList[j].getRectangle().X - bubbleList[bubbleList.Count() - 1].getRectangle().X <= bubbleSize.Width &&
                        bubbleList[j].getRectangle().X - bubbleList[bubbleList.Count() - 1].getRectangle().X >= -bubbleSize.Width)
                    {
                        matchFound = true;
                        bubbleList[bubbleList.Count() - 1].setRectangleY(bubbleList[j].getRectangle().Y + bubbleSize.Height, bubbleSize);
                        if (bubbleList[j].getRectangle().Y + bubbleSize.Height > totalHeight)
                            totalHeight = bubbleList[j].getRectangle().Y + bubbleSize.Height;
                        bubbleFlying = false;
                    }
                }

                if (!bubbleFlying)
                {
                    deleteTouchingBubblesOfSameColor(flyingBubble);
                    if (totalHeight + 3 * bubbleSize.Height > myCanvas.Height)
                    {
                        MessageBox.Show("You loose! Your score is: " + points.ToString() + ". And you reached level: " + level.ToString());
                        animationTimer.Stop();
                    }
                }
            }
            g.Clear(Color.AliceBlue);            
            for (int i = 0; i < bubbleList.Count(); i++)
                bubbleList[i].DrawBubble(g);
            for (int j = 0; j < nextBubbles.Count(); j++)
                nextBubbles.ElementAt(j).DrawBubble(g);
            g.DrawLine(launchLinePen, p1, p2);

            if (bubbleList.Count == 0) //&& level < 2)
            {
                level++;
                points += 500;
                loadLevel(level);
            }
            /*else if (level > 1)
                MessageBox.Show("You Win! \n Your score is: " + points.ToString());*/
            PointsLabel.Text = points.ToString();
            tick++;
            this.Refresh();
        }


        private void FaerieBubbles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right && theta < Math.PI/2 - Math.PI/20 )
                theta += Math.PI /20;
            if (e.KeyCode == Keys.Left && theta > -Math.PI / 2 + Math.PI/20)
                theta -= Math.PI /20;
            if (e.KeyCode == Keys.Space)
                if (bubbleFlying != true)
                {
                    launchBubble = true;
                    points -= 10;
                }

        }

        private void loadLevel(int lev)
        {
            myCanvas = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height - 25,
               System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            numCanvasChanges = 0;
            Graphics g = Graphics.FromImage(myCanvas);
            g.Clear(Color.AliceBlue);
            //switch(lev)
            //{
            //    case 1:
                    bubbleList = new List<Bubbles>();
                    //Generate Colors
                    Random random = new Random();
                    int c1 = random.Next(1, 5);
                    int c2 = random.Next(1, 5);
                    int c3 = random.Next(1, 5);
                    int c4 = 0;
                    int c5 =0;
                    while (c1 == c2)
                        c2 = random.Next(1, 5);
                    while (c3 == c1 || c3 == c2)
                        c3 = random.Next(1, 5);
                    if (lev > 3)
                    {
                        c4 = random.Next(1, 5);
                        while (c4 == c1 || c4 == c2 || c4 == c3)
                            c4 = random.Next(1, 5);
                    }
                    if (lev > 5)
                    {
                        c5 = random.Next(1, 5);
                        while (c5 == c1 || c5 == c2 || c5 == c3)
                            c5 = random.Next(1, 5);
                    }

                    colorsThisLevel = new List<Color>();
                    colorsThisLevel.Add(intToColor(c1));
                    colorsThisLevel.Add(intToColor(c2));
                    colorsThisLevel.Add(intToColor(c3));
                    if (lev > 3)
                        colorsThisLevel.Add(intToColor(c4));
                    if (lev > 5)
                        colorsThisLevel.Add(intToColor(c5));

                    PointF bubPoint = new PointF(myCanvas.Width/2-4*bubbleSize.Width, 0);
                    if(lev <= 3) 
                        bubbleList.Add(new Bubbles(intToColor(c2), bubPoint, bubbleSize));
                    else
                        bubbleList.Add(new Bubbles(intToColor(c4), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    bubPoint.X += bubbleSize.Width;
                    if(lev <= 5)
                        bubbleList.Add(new Bubbles(intToColor(c1), bubPoint, bubbleSize));
                    else
                        bubbleList.Add(new Bubbles(intToColor(c5), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    for (int i = 0; i < 4; i++)
                    {
                        bubPoint.X += bubbleSize.Width;
                        bubbleList.Add(new Bubbles(intToColor(c2), bubPoint, bubbleSize));
                        bubblesInExistance++;
                    }
                    bubPoint.X += bubbleSize.Width;
                    bubbleList.Add(new Bubbles(intToColor(c1), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    bubPoint.X += bubbleSize.Width;
                    bubbleList.Add(new Bubbles(intToColor(c2), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    bubPoint.X = myCanvas.Width/ 2 - bubbleSize.Width/ 2 - bubbleSize.Width;
                    bubPoint.Y += bubbleSize.Height;
                    bubbleList.Add(new Bubbles(intToColor(c3), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    for (int i = 0; i < 2; i++)
                    {
                        bubPoint.X += bubbleSize.Width;
                        bubbleList.Add(new Bubbles(intToColor(c3), bubPoint, bubbleSize));
                        bubblesInExistance++;
                    }
                    g = Graphics.FromImage(myCanvas);
                    for (int i = 0; i < bubbleList.Count(); i++)
                    {
                        bubbleList[i].DrawBubble(g);
                    }
                    totalHeight = 2 * bubbleSize.Height;

                    //break;
                /*case 2:
                    bubbleList = new List<Bubbles>();
                    //Generate Colors
                    random = new Random();
                    c1 = random.Next(1, 5);
                    c2 = random.Next(1, 5);
                    c3 = random.Next(1, 5);
                    while (c1 == c2)
                        c2 = random.Next(1, 5);;
                    while (c3 == c1 || c3 == c2)
                        c3 = random.Next(1, 5);;
                    colorsThisLevel = new List<Color>();
                    colorsThisLevel.Add(intToColor(c1));
                    colorsThisLevel.Add(intToColor(c2));
                    colorsThisLevel.Add(intToColor(c3));

                    bubPoint = new PointF(myCanvas.Width/2-5*bubbleSize.Width, 0);
                    for (int i = 0; i < 1; i++)
                    {
                        bubbleList.Add(new Bubbles(intToColor(c2), bubPoint, bubbleSize));
                        bubblesInExistance++;
                        bubPoint.X += bubbleSize.Width;
                    }
                    
                    bubbleList.Add(new Bubbles(intToColor(c1), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    for (int i = 0; i < 6; i++)
                    {
                        bubPoint.X += bubbleSize.Width;
                        bubbleList.Add(new Bubbles(intToColor(c2), bubPoint, bubbleSize));
                        bubblesInExistance++;
                    }
                    bubPoint.X += bubbleSize.Width;
                    bubbleList.Add(new Bubbles(intToColor(c1), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    for (int i = 0; i < 1; i++)
                    {
                        bubPoint.X += bubbleSize.Width;
                        bubbleList.Add(new Bubbles(intToColor(c2), bubPoint, bubbleSize));
                        bubblesInExistance++;
                    }
                    bubPoint.X = myCanvas.Width/ 2 - bubbleSize.Width/ 2 - bubbleSize.Width*4;
                    bubPoint.Y += bubbleSize.Height;
                    bubbleList.Add(new Bubbles(intToColor(c3), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    bubPoint.X += bubbleSize.Width;
                    bubbleList.Add(new Bubbles(intToColor(c3), bubPoint, bubbleSize));
                    bubblesInExistance++;
                    bubPoint.X += 2 * bubbleSize.Width; 
                    for (int i = 0; i < 2; i++)
                    {
                        bubbleList.Add(new Bubbles(intToColor(c3), bubPoint, bubbleSize));
                        bubblesInExistance++;
                        bubPoint.X += bubbleSize.Width;
                    }

                    bubPoint.X += bubbleSize.Width;
                    for (int i = 0; i < 2; i++)
                    {
                        bubbleList.Add(new Bubbles(intToColor(c3), bubPoint, bubbleSize));
                        bubblesInExistance++;
                        bubPoint.X += bubbleSize.Width;
                    }

                    g = Graphics.FromImage(myCanvas);
                    for (int i = 0; i < bubbleList.Count(); i++)
                    {
                        bubbleList[i].DrawBubble(g);
                    }

                    break; */
           // }
            
            nextBubbles = new Queue<Bubbles>();
            PointF p = new PointF(myCanvas.Width / 2 - bubbleSize.Width / 2 - bubbleSize.Width, myCanvas.Height - bubbleSize.Height);
            for (int i = 0; i < 6; i++)
            {
                p.X += bubbleSize.Width;
                int c = rand.Next(0,colorsThisLevel.Count()); 
                Color nextColor = colorsThisLevel[c];
                nextBubbles.Enqueue(new Bubbles(nextColor, p, bubbleSize));
            }
        }
        private Color intToColor(int n)
        {
            switch (n)
            {
                case 1:
                    return orange;
                case 2:
                    return blue;
                case 3:
                    return purple;
                case 4:
                    return green;
                case 5:
                    return yellow;
            }
            return Color.AliceBlue;
        }

        private void deleteTouchingBubblesOfSameColor(Bubbles bubble)
        {
            for (int j = 0; j < bubbleList.Count(); j++)
            {
                if (bubbleList[j].getRectangle() != bubble.getRectangle())
                {
                    Bubbles temp =bubbleList[j];
                    if (bubble.getRectangle().Y - bubbleList[j].getRectangle().Y == bubbleSize.Height
                            && bubbleList[j].getRectangle().X - bubble.getRectangle().X <= bubbleSize.Width &&
                         bubbleList[j].getRectangle().X - bubble.getRectangle().X >= -bubbleSize.Width)
                    {
                        if (bubbleList[j].bubColor == bubble.bubColor)
                        {
                            bubbleList.Remove(bubble);
                            points += 10; 
                            deleteTouchingBubblesOfSameColor(bubbleList[j]);
                            if(bubbleList.Contains(temp))
                            {
                                bubbleList.Remove(temp);
                                points += 10;
                                totalHeight = 0;
                                for (int i = 0; i < bubbleList.Count(); i++)
                                {
                                    if (bubbleList[i].getRectangle().Y > totalHeight)
                                        totalHeight = bubbleList[i].getRectangle().Y;
                                }
                            }
                            
                        }
                    }
                    else if (bubble.getRectangle().Y - bubbleList[j].getRectangle().Y == 0
                            && ((bubbleList[j].getRectangle().X - bubble.getRectangle().X <= bubbleSize.Width + 10 && bubbleList[j].getRectangle().X - bubble.getRectangle().X > 0) ||
                         bubbleList[j].getRectangle().X - bubble.getRectangle().X >= -bubbleSize.Width - 10 && bubbleList[j].getRectangle().X - bubble.getRectangle().X < 0))
                    {
                        if (bubbleList[j].bubColor == bubble.bubColor)
                        {
                            bubbleList.Remove(bubble);
                            totalHeight = 0;
                            for (int i = 0; i < bubbleList.Count(); i++)
                            {
                                if (bubbleList[i].getRectangle().Y > totalHeight)
                                    totalHeight = bubbleList[i].getRectangle().Y;
                            }
                            points += 10;
                            deleteTouchingBubblesOfSameColor(bubbleList[j]);
                            if (bubbleList.Contains(temp))
                            {
                                bubbleList.Remove(temp);
                                totalHeight = 0;
                                for (int i = 0; i < bubbleList.Count(); i++)
                                {
                                    if (bubbleList[i].getRectangle().Y > totalHeight)
                                        totalHeight = bubbleList[i].getRectangle().Y;
                                }
                                points += 10;
                            }
                           
                        }
                    }
                }

            }
            
        }
        class Bubbles
        {
            public readonly Color bubColor; 
            private RectangleF circle;

            public Bubbles()
            {
                bubColor = Color.AliceBlue;
                circle = new RectangleF();
            }
            public Bubbles(Color c, RectangleF r)
            {
                bubColor = c;
                circle = new RectangleF();
            }
            public Bubbles(Color c, PointF loc, SizeF size)
            {
                circle = new RectangleF(loc, size);
                bubColor = c; 
            }
            public void DrawBubble(Graphics g)
            {
                SolidBrush b = new SolidBrush(bubColor);
                g.FillEllipse(b, circle);
            }
            public void setloc(PointF newPoint, SizeF size)
            {
                circle = new RectangleF(newPoint, size);
            }
            public RectangleF getRectangle()
            {
                return circle;
            }
            public void changeRectangleX(float change, SizeF size)
            {
                PointF p = new PointF(circle.Location.X + change, circle.Y);
                circle = new RectangleF(p, size);
            }
            public void setRectangleY(float newY, SizeF size)
            {
                PointF p = new PointF(circle.Location.X, newY);
                circle = new RectangleF(p, size);
            }
            
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To Play:  \n The Goal: is to pop all of the bubbles. Any two bubbles of the same color that are touching will pop. \n Use Arrow Keys to change the angle of launch.  \n Use Space Bar to launch a balloon.");
        }

    }
}
