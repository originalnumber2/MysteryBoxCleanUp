using System;
namespace MysteryBoxCleanUp
{
    public class VerticalMotor
    {
		public VerticalMotor()
        {
        }

		void btnVerRun_Click(object sender, EventArgs e)
        {
            String VerMessage;
            int VerPos = (int)nmVerTurns.Value;
            VerMessage = String.Empty;
             
            if (rbVerContinuous.Checked)
            {
                VerMessage = "E MC H";
                VerMessage += "+";

                VerMessage += "A" + nmVerAcc.Value.ToString() + " V" + nmVerVel.Value.ToString() + " G\r";
            }
            else
            {
                VerMessage = "E MN A" + nmVerAcc.Value.ToString() + " V" + nmVerVel.Value.ToString() + " D";
                VerMessage += VerPos.ToString() + " G\r";
            }

            VerPort.Write(VerMessage);
        }
        void btnVerLowerStage_Click(object sender, EventArgs e)
        {
            String VerMessage;
            int VerPos = (int)nmVerTurns.Value;
            VerMessage = String.Empty;

            if (rbVerContinuous.Checked)
            {
                VerMessage = "E MC H";
                VerMessage += "-";
                VerMessage += "A" + nmVerAcc.Value.ToString() + " V" + nmVerVel.Value.ToString() + " G\r";
            }
            else
            {
                VerMessage = "E MN A" + nmVerAcc.Value.ToString() + " V" + nmVerVel.Value.ToString() + " D";
                VerMessage += "-";
                VerMessage += VerPos.ToString() + " G\r";
            }

            VerPort.Write(VerMessage);
        }
        void btnVerCon_Click(object sender, EventArgs e)
        {
            if (!isVerCon)//connect to vertical motor
            {
                VerConnect();
            }
            else//disconnect motor
            {
                VerDisconnect();
            }
        }
        void btnVerStop_Click(object sender, EventArgs e)
        {
            VerPort.Write("E S\r");
        }
        void VerConnect()
        {
            string VerMessage;
            //Clear out port
            VerPort.ReadExisting(); //this was edited out

            //Query the motor and establish RS-232 control
            VerPort.Write("E ON 1R\r"); //this was edited out
            Thread.Sleep(300); //this was edited out

            //Turn off limits
            VerPort.Write("1LD3\r"); //this was edited out
            Thread.Sleep(30); //this was edited out

            //Initialize Now
            VerPort.Write("1E 1MN 1A10 1V10 1D0 G\r"); //this was edited out
            Thread.Sleep(30); //this was edited out

            //Clear out port and place in holding string
            VerMessage = VerPort.ReadExisting(); //this was edited out

            if (VerMessage.Length < 2) //this was edited out
            {
                WriteMessageQueue("Connection to vertical motor failed");
            }
            else //this was edited out
            {
                //Update the boolean
                isVerCon = true;

                //Show the connection label
                btnVerCon.BackColor = System.Drawing.Color.Green;


                //Show the vertical controls
                boxVer.Visible = true;

                //check to see if autozero should be allowed
                // if (isSenCon && isStrainCon) //IsDynCon removed Brian
                {
                    //btnAutoZero.Enabled = true;
                }
            }
        }//Connect to the Vertical Motor
        void VerDisconnect()
        {
            //Turn off the motor for starters
            VerPort.Write("S\r");
            Thread.Sleep(300);
            VerPort.Write("OFF\r");
            isVerCon = false;
            //show that vertical motor is disconected on gui
            btnVerCon.BackColor = System.Drawing.Color.Red;
            btnVerCon.ForeColor = System.Drawing.Color.White;
            boxVer.Visible = false;
            //btnAutoZero.Enabled = false;
        }//Disconect and turn off the Vertical Motor
        void StopVer() //Stop the vertical Motor
        {
            VerPort.Write("S\r");
        }
    }
}
