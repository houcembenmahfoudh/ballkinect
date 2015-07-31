using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;


namespace Microsoft.Samples.Kinect.BodyBasics
{
    class Image_display
    {
        private int windowHeight;
        private int windowWeight;
        private Image image;
        public Point3D speed;
        public Point3D oldSpeed;
        private Point3D oldPosition;
        private double posZ=100.0;
        private double t;

        const double gravity = 120.81;
        const double delta_t = 0.9;
        const int left_hand = -1;
        const int right_hand = 1;
        public Rect rect;

        public Image_display( int windowHeight,int windowWeight, Image im, double posX, double posY, double width, double height )
        {
            this.windowHeight = windowHeight;
            this.windowWeight = windowWeight;
            this.image = im;
            this.isHeld = false;
            this.isHeldBy = left_hand;
          
            this.rect = new Rect(posX, posY, width, height);
            this.speed = new Point3D(0.0, -40.0,0.0);
            this.oldSpeed = new Point3D(0.0, 0.0,0.0);
            this.oldPosition = new Point3D(posX, posY,posZ);
            this.t = 0.03;
        }

        public Boolean isHeld { get; set; }
        public int isHeldBy { get; set; }

        public void Draw(DrawingContext drawingContext)
        {
             this.rect.Height = this.windowHeight * 20 / this.posZ;
            this.rect.Width = this.windowHeight * 20 / this.posZ;
            drawingContext.DrawImage(this.image.Source, this.rect);
        }

        public void Move(HandState leftHandState, Point3D leftHandPosition3D, HandState rightHandState, Point3D rightHandPosition3D)
        {
            Point leftHandPosition = new Point(leftHandPosition3D.X, leftHandPosition3D.Y);
            Point rightHandPosition = new Point(rightHandPosition3D.X, rightHandPosition3D.Y);

            //this.FollowTheHand(left_hand, leftHandPosition3D);
            //this.FollowTheHand(left_hand, leftHandPosition);
            //this.MoveNaturally();
            if (this.rect.Contains(leftHandPosition) && this.rect.Contains(rightHandPosition))
            {
                if (leftHandState == HandState.Closed && rightHandState == HandState.Closed)
                {
                    if (this.isHeldBy == left_hand)
                    {
                        // object was previously in the left hand, so continue with it
                        if (ballMatchZ(leftHandPosition3D.Z, posZ))
                            this.FollowTheHand(left_hand, leftHandPosition3D);
                        else
                            this.MoveNaturally();
                    }
                    else if (this.isHeldBy == right_hand)
                    {
                        // object was previously in the right hand, so continue with it
                        if (ballMatchZ(rightHandPosition3D.Z, posZ))
                            this.FollowTheHand(right_hand, rightHandPosition3D);
                        else
                            this.MoveNaturally();
                    }
                    else
                    {
                        // choose randomly the hand, e.g. the left one
                        if (ballMatchZ(leftHandPosition3D.Z, posZ))
                            this.FollowTheHand(left_hand, leftHandPosition3D);
                        else
                            this.MoveNaturally();
                    }
                }
                else if (leftHandState == HandState.Closed && rightHandState != HandState.Closed)
                {
                    // object is in the left hand
                    if (ballMatchZ(leftHandPosition3D.Z, posZ))
                        this.FollowTheHand(left_hand, leftHandPosition3D);
                    else
                        this.MoveNaturally();
                }
                else if (leftHandState != HandState.Closed && rightHandState == HandState.Closed)
                {
                    // object is in the right hand
                    if (ballMatchZ(rightHandPosition3D.Z, posZ))
                        this.FollowTheHand(right_hand, rightHandPosition3D);
                    else
                        this.MoveNaturally();
                }
                else
                {
                    // no hand is open
                    this.MoveNaturally();
                }
            }
            else if (this.rect.Contains(leftHandPosition))
            {
                if (leftHandState == HandState.Closed) {
                    // object is in the left hand
                    if (ballMatchZ(leftHandPosition3D.Z, posZ))
                        this.FollowTheHand(left_hand, leftHandPosition3D);
                    else
                        this.MoveNaturally();
                }
                else
                {
                    // left hand is not open
                    this.MoveNaturally();
                }
            }
            else if (this.rect.Contains(rightHandPosition))
            {
                if (rightHandState == HandState.Closed) {
                    // object is in the right hand
                    if (ballMatchZ(rightHandPosition3D.Z, posZ))
                        this.FollowTheHand(right_hand, rightHandPosition3D);
                    else
                        this.MoveNaturally();
                }
                else
                {
                    // right hand is not open
                    this.MoveNaturally();
                }
            }
            else
            {
                // no hand match the object
                this.MoveNaturally();
            }
            Console.WriteLine("object Z " + posZ);
        }

        private void FollowTheHand(int hand, Point3D handPosition)
        {
            this.isHeld = true;
            this.isHeldBy = hand;

            // update speed
            this.oldSpeed.X = this.speed.X;
            this.oldSpeed.Y = this.speed.Y;
            this.oldSpeed.Z = this.speed.Z;
            this.speed.X = ((handPosition.X - (this.rect.Width / 2)) - this.oldPosition.X) / t / 2;
            this.speed.Y = ((handPosition.Y - (this.rect.Height / 2)) - this.oldPosition.Y) / t / 2;
            this.speed.Z = (posZ - this.oldPosition.Z) / t / 2;
            // update position
            this.oldPosition.X = this.rect.X;
            this.oldPosition.Y = this.rect.Y;
            this.oldPosition.Z = this.posZ;
            this.rect.X = handPosition.X - (this.rect.Width / 2);
            this.rect.Y = handPosition.Y - (this.rect.Height / 2) ;
            this.posZ = handPosition.Z;
        }

        private void MoveNaturally()
        {

            this.isHeld = false;
           
            // update position
            this.oldPosition.X = this.rect.X;
            this.oldPosition.Y = this.rect.Y;
            this.oldPosition.Z = this.posZ;
            this.rect.X = this.oldSpeed.X * t + this.oldPosition.X;
            this.rect.Y = (0.5 * gravity * t * t) + this.oldSpeed.Y * t + this.oldPosition.Y;
            this.posZ = this.oldSpeed.Z * t + this.oldPosition.Z;
            if (this.rect.X < 10 || this.rect.X > this.windowWeight-this.rect.Width)
                this.speed.X = -this.speed.X;
            if (this.rect.Y < 10 || this.rect.Y > this.windowHeight-this.rect.Height)
                this.speed.Y = -this.speed.Y;
            if (this.posZ * 4 / 500 < 0.6 || this.posZ * 4 / 500 > 3)
                this.speed.Z = 0;
            this.oldSpeed.X = this.speed.X;
            this.speed.X = this.oldSpeed.X;
            // update speed
            this.oldSpeed.Y = this.speed.Y;
            this.oldSpeed.Z = this.speed.Z;
            this.speed.Y = (gravity * t) + this.oldSpeed.Y;
            this.speed.Z = this.oldSpeed.Z;

         /*   if (this.rect.Y + this.rect.Height > windowHeight-100)
            {
                this.rect.Y = windowHeight-100 - this.rect.Height;
                this.speed.X = 0;
                this.speed.Z = 0;
            }*/

            //this.rect.X = this.rect.X + 1;
            //this.rect = new Rect(this.rect.X + 10, this.rect.Y, this.rect.Width, this.rect.Height);
        }

        public Rect getRect() {
            return this.rect;
        }

        public ImageSource getImageSource()
        {
            return this.image.Source;
        }

        private Boolean ballMatchZ(double handDepth, double ballDepth)
        {
            if (ballDepth < handDepth + 20 && ballDepth > handDepth - 20)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
