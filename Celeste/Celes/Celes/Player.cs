using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste
{
    public class Player
    {
        #region Variables

        private readonly ContentManager Content; ///Manages Content
        private readonly _Hitbox prevHitbox;
        private readonly _Hitbox Hitbox;

        ///Sounds
        private Song[] WalkSounds = new Song[5]; //Self explanatory variablels
        private Song[] ClimbSounds = new Song[5];
        private Song JumpSound;
        private Song DashSound;
        private int SoundCycler = 0; //Holds sound index

        ///Animation Vars
        private List<Rectangle> SpriteList = new List<Rectangle>(); //List that holds dimensions and locations for each frame
        private Rectangle current_frame; //Holds the rectangle for the location the current sprite must be cut from ( out of the spritesheet )
        private SpriteEffects SpriteFlipper = SpriteEffects.None; //Flips the sprite texture when appropriate
        private Texture2D current_spritesheet; //Holds the current_spritesheet that sprite frames will be cut from ( animation )
        private float FrameTimer = 0; //Times how much time spent on each frame
        private float MaxFrameTime; //Time that each animation frame lasts
        private int FrameCount = 0; //Counts what frame the character is currently on
        private int FrameNum; //Hold the number of frames for the current animation state

        ///Character Attributes
        private Vector2 spawnpoint;
        private Vector2 PositionRounded;
        private float scale; //Holds the scale multiplier of the player sprite
        private Vector2 Position; //Position of the character
        private Vector2 prevPosition; //Position in the previous frame
        private Vector2 dvelocity; //Change in Velocity
        private Vector2 velocity; //Player Velocity
        private Color playerColor = Color.White; //Holds the player colour
        private Vector2 InputDirection = Vector2.Zero; //Holds the direction of input

        ///Player States
        private bool IsGrounded; //Self explanatory

        ///Player Timers
        private float JumpTimer = 0;
        private float DashTimer = 0;
        private float DashCooldownTimer = 0;

        ///Max Timer Constants
        private const float MaxJumpTime = 0.2f; //Jump Grace Time Constant
        private const float MaxDashTime = 0.15f; //Max Time Player can dash for
        private const float DashCooldownTime = 0.1f; //Time for dash cooldown

        ///Force/Speed Constants
        private static float SpeedConstant = 0.05f;
        private const float JumpForce = 20f;
        private float DashForce = 30f;
        private float Gravity = 2.5f;
        private float ClimbSpeed = 5f;
        private const float accel = 1.2f; //Acceleration Constant
        private const float decel = 3f; //Deceleration Constant

        ///Input Keys
        public Keys UpMoveKey;
        public Keys LeftMoveKey;
        public Keys DownMoveKey;
        public Keys RightMoveKey;
        public Keys JumpKey;
        public Keys DashKey;
        public Keys ClimbKey;

        #endregion

        #region Enums

        public enum CharacterState
        {
            Normal = 1, //When not in any other states
            Jumping = 2,
            Dashing = 3,
            Climbing = 4,
            Death = 5
        }

        private CharacterState playerState = 0; //Player State ( Enum ) variable

        //CharacterStates
        private const CharacterState StNormal = CharacterState.Normal;
        private const CharacterState StJumping = CharacterState.Jumping;
        private const CharacterState StDashing = CharacterState.Dashing;
        private const CharacterState StClimbing = CharacterState.Climbing;
        private const CharacterState StDeath = CharacterState.Death;

        //=======================================================================================================================================
        public enum AnimationState
        {
            Idle = 0,
            Running = 1,
            Jumping = 2, //At any point when the player is in the air, not just jumping
            Dashing = 3,
            Climbing = 4
        }

        private AnimationState animState = 0; //Holds the players current animation state
        private AnimationState prev_animState = 0;

        //Anim States Constants
        private const AnimationState AnimIdle = AnimationState.Idle;
        private const AnimationState AnimRunning = AnimationState.Running;
        private const AnimationState AnimJumping = AnimationState.Jumping;
        private const AnimationState AnimDashing = AnimationState.Dashing;
        private const AnimationState AnimClimbing = AnimationState.Climbing;

        #endregion

        public Player(ContentManager contentManager, Vector2 position, Vector2 velocity, float scale)
        {
            prevHitbox = new _Hitbox(position, scale);
            Hitbox = new _Hitbox(position, scale);
            Content = contentManager;
            spawnpoint = position;
            this.velocity = velocity;
            this.scale = scale;
            Position = position;
            LoadControls(); //Loads the keybind data
            LoadSounds(); //LLoads the sound data
        }

        #region Main Functions

        ///--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public void Update(GameTime gameTime)
        {
            PlayerMovement(gameTime); //Calls separate function to handle player movement
            Animation(gameTime); //Cals separate function to handle player animation
            UpdateSound(); //Calls separate function to handlle sound effects
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PositionRounded.X = (scale / 2) * (float)Math.Floor(Position.X / (scale / 2)); //Rounds the position to the nearest third of a pixel
            PositionRounded.Y = (scale / 2) * (float)Math.Floor(Position.Y / (scale / 2)); //Rounds the position to the nearest third of a pixel
            spriteBatch.Draw(current_spritesheet, PositionRounded, current_frame, playerColor, 0f, new Vector2(current_frame.Width / 2, current_frame.Height / 2), scale, SpriteFlipper, 0); //Draws the player
        }

        public Matrix ReturnTransformationMatrix(int viewportWidth, int viewportHeight, float levelWidth, float levelHeight)
        {
            float cameraX = Math.Clamp(Position.X - viewportWidth / 2f, 0f, Math.Max(levelWidth - viewportWidth, 0f));
            float cameraY = Math.Clamp(Position.Y - viewportHeight / 2f, 0f, Math.Max(levelHeight - viewportHeight, 0f));
            var cameraPosition = new Vector2(-cameraX, -cameraY);
            return Matrix.CreateTranslation(cameraPosition.X, cameraPosition.Y, 0f);
        }

        #endregion

        #region Player Movement

        ///--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------         PLAYER MOVEMENT CODE
        private void PlayerMovement(GameTime gameTime)
        {
            GetPlayerIsGrounded(); //Updates IsGrounded() state
            GetPlayerIsColliding(); //Updates Hitbox.IsColliding states
            float dt = gameTime.ElapsedGameTime.Milliseconds; //Gets Time since last frame
            KeyboardState keystate = Keyboard.GetState(); //Gets keyboard inputs
            dvelocity = Vector2.Zero; //Resets dvelocity to 0
            PlayerTimerManager(gameTime);
            PlayerStateManager(keystate); //Sets player states ( e.g. IsGrounded, IsDashing ... )
            InputDirection = GetInputDirection(keystate);

            if (keystate.GetPressedKeyCount() > 0) //Checks if keys are being pressed
            {
                InputMovement(keystate);
            }

            PlayerDeceleration(); //Calculates player deceleration
            VelocityCap(); //Sets velocity to max velocity.
            velocity.Y += dvelocity.Y; //Adds dvelocity to velocity } X
            velocity.X += dvelocity.X; //Adds dvelocity to velocity } Y
            prevPosition = Position; ///Updates prevPosition right before the new Position is calculated
            Position.Y += (velocity.Y * dt * SpeedConstant); //Changes Y Position value by velocity
            Position.X += (velocity.X * dt) * SpeedConstant * 0.8f; //Changes X Position value by velocity
            prevHitbox.Center = prevPosition;
            Hitbox.Center = Position;
            CheckAllVerticies();
        } ///HANDLES PLAYER MOVEMENT

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void InputMovement(KeyboardState keyState)
        {
            if (playerState != StDashing && playerState != StClimbing) //Prevents players from walking/moving mid-dash
            {
                dvelocity.X = InputDirection.X * accel; //Horizontal movement
            }
            else if (playerState == StClimbing)
            {
                Climb(keyState);
            }

            if (InputDirection != Vector2.Zero && keyState.IsKeyDown(DashKey) && DashTimer == 0) //If not dashing and a direction is being held
            {
                Dash();
            }
            else if ((((SpriteFlipper == 0 && Hitbox.IsCollidingRight) || (SpriteFlipper == (SpriteEffects)1 && Hitbox.IsCollidingLeft)) && playerState == StNormal && keyState.IsKeyDown(ClimbKey))) //Checks if player is in contact with wall,facing it, holding climb key AND also not performing other actions
            {
                playerState = StClimbing;
            }

            //Jump logic
            if (keyState.IsKeyDown(JumpKey))
            {
                Jump();
            }
        } ///CHANGES DVELOCITY/VELOCITY ACCORDING TO PLAYER INPUT

        private void PlayerTimerManager(GameTime gameTime)
        {
            float dt = gameTime.ElapsedGameTime.Milliseconds;
            JumpTimer = (IsGrounded && playerState != StJumping) || playerState == StClimbing ? 0 : !IsGrounded && playerState != StJumping ? MaxJumpTime : JumpTimer + dt / 1000; //Resets JumpTimer if touching ground, else timer is incremented

            if (playerState == StDashing && (DashTimer < MaxDashTime)) //If player is dashing and timer is not up
            {
                DashTimer += dt / 1000; //Timer is iterated
            }
            else
            {
                if (DashTimer >= MaxDashTime || (DashCooldownTimer != 0 && DashCooldownTimer < DashCooldownTime)) //Checks if the player has recently come out of the dash state, or if the dash is on cooldown
                {
                    DashCooldownTimer += dt / 1000; //Adds to coolldown timer
                    DashTimer = -1f; ///Sets DashTimer to -1 so that it is less than MaxDashTime to make the first part of the if statement false
                }
                else
                {
                    velocity = DashCooldownTimer != 0 ? new Vector2(Math.Sign(velocity.X) * 15f, Math.Sign(velocity.Y) * 15f) : velocity;
                    DashCooldownTimer = 0; //Resets Cooldown timer once done
                    DashTimer = IsGrounded ? 0 : DashTimer; //Checks if player is grounded;
                }
            }
        } ///UPDATES PLAYER TIMERS ( JUMP TIMER, DASH TIMER ETC. )

        private void PlayerStateManager(KeyboardState keystate)
        {
            switch (playerState)
            {
                case CharacterState.Jumping:
                    if (!(JumpTimer != 0 && JumpTimer < MaxJumpTime && keystate.IsKeyDown(JumpKey))) //Checks if currently in JumpState, if so checks if the player SHOULD be in this state, switches accordingly
                    {
                        playerState = StNormal;
                        JumpTimer = IsGrounded ? 0 : MaxJumpTime;
                        playerColor = Color.White;
                    }
                    else
                    {
                        playerColor = Color.Cyan;
                    }
                    break;

                case CharacterState.Dashing: ////Checks if currently in JumpState, if so checks if the player SHOULD be in this state, switches accordingly
                    if (!(DashTimer != 0 && DashTimer < MaxDashTime))
                    {
                        playerState = StNormal;
                        playerColor = Color.White;
                    }
                    else
                    {
                        playerColor = Color.Orchid;
                    }
                    break;

                case CharacterState.Climbing:
                    if (!keystate.IsKeyDown(ClimbKey) || !(Hitbox.IsCollidingLeft || Hitbox.IsCollidingRight))
                    {
                        playerState = StNormal;
                        playerColor = Color.White;
                    }
                    else
                    {
                        playerColor = Color.GreenYellow;
                    }
                    break;

                default:
                    playerState = StNormal;
                    playerColor = Color.White;
                    break;
            }
        } ///UPDATES PLAYER STATES ( ISGROUNDED, ISDASHING ETC. )

        private Vector2 GetInputDirection(KeyboardState keystate)
        {
            Vector2 inputDirection = Vector2.Zero;
            if (keystate.IsKeyDown(UpMoveKey)) inputDirection.Y -= 1; //Direct Up
            if (keystate.IsKeyDown(DownMoveKey)) inputDirection.Y += 1; //Direct Down
            if (keystate.IsKeyDown(LeftMoveKey)) inputDirection.X -= 1; //Direct Left
            if (keystate.IsKeyDown(RightMoveKey)) inputDirection.X += 1; //Direct Right
            return inputDirection;
        } ///CALCULATES DIRECTIONAL INPUT

        private void PlayerDeceleration()
        {
            //Decel X
            if (dvelocity.X == 0 && velocity.X != 0 && playerState != StDashing && playerState != StClimbing)
            {
                dvelocity.X += -decel * Math.Sign(velocity.X); //Sets dveloicty to decel value AGAINST velocity direction
                velocity.X = Math.Abs(velocity.X + dvelocity.X) < 3 ? 0 : velocity.X + dvelocity.X; //Rounds velocity down to 0 if close
                dvelocity.X = 0;
            }

            //Decel Y
            if (!IsGrounded && playerState == StNormal) //Checks if player is currently grounded, and not in any other states ( normal state )
            {
                dvelocity.Y += Gravity; //Vertical Deceleration ( Gravity )
            }
        } ///CALCULATES DECELERATION AND CHANGES DVELOCITY ACCORDINGLY

        private void VelocityCap()
        {
            //Velocity Cap
            if (playerState != StDashing && playerState != StClimbing) //Checks if velocity would exceed max velocity
            {
                dvelocity.X += -Math.Sign(velocity.X) * velocity.X * velocity.X * 0.003f; //Sets dvelocity to the difference between (absolute) velocity and max velocity
                if (Keyboard.GetState().IsKeyDown(DownMoveKey))
                {
                    dvelocity.Y += -Math.Sign(velocity.Y) * velocity.Y * velocity.Y * 0.002f; //Sets dvelocity to the difference between (absolute) velocity and max velocity
                }
                else
                {
                    dvelocity.Y += -Math.Sign(velocity.Y) * velocity.Y * velocity.Y * 0.003f; //Sets dvelocity to the difference between (absolute) velocity and max velocity
                }
            }
        }

        private void Jump()
        {
            if (playerState == StDashing && JumpTimer < MaxJumpTime) //Checks if jump occured during a dash
            {
                velocity.Y = 2f * -JumpForce; //Increased jump strength as it only occurs in a single frame
                velocity.X = 1.4f * DashForce * InputDirection.X; //Gives horizontoal boost
                JumpTimer = MaxJumpTime; //Sets timer to max to prevent player from increasing jump height via. Holding the jump key
                playerState = StJumping; //Sets playerstate to normal because the player cannot increase the jump time after
            }
            else if (playerState == StClimbing) //Checks if player is climbing
            {
                playerState = (Hitbox.IsCollidingLeft && InputDirection.X == 1) || (Hitbox.IsCollidingRight && InputDirection.X == -1) ? StNormal : StJumping; //Set to StNormal for wall hop, StJumping for vertical hop
                JumpTimer = 0;
                Jump(); //Recursive call.
            }
            else if ((Hitbox.IsCollidingLeft || Hitbox.IsCollidingRight) && !IsGrounded && playerState != StJumping) //Checks if player is colliding with wall, isn't grounded and isn't already jumping
            {
                JumpTimer = 0;
                velocity.Y = -JumpForce; //Makes it wall jump if true
                velocity.X = SpriteFlipper == 0 ? -2 * JumpForce : JumpForce;
                playerState = StJumping;
            }
            else if (JumpTimer < MaxJumpTime) //Normal jump
            {
                velocity.Y = -JumpForce; //Sets vertical velocity to the jump force
                playerState = StJumping; //Sets playerstate to jumping
            }
        }

        private void Dash()
        {
            velocity = Vector2.Zero;
            dvelocity = Vector2.Zero;

            if (InputDirection.X != 0 && InputDirection.Y != 0) //Diagonal Dashing
            {
                velocity.Y = DashForce * (float)Math.Sin(45) * InputDirection.Y; //Multiply by Sin45 to keep diagonal dash distance same as horizontal dash distance
                velocity.X = DashForce * (float)Math.Sin(45) * InputDirection.X;
            }
            else
            {
                velocity.Y = DashForce * InputDirection.Y; //Vertical Dashing
                velocity.X = DashForce * InputDirection.X;
            }

            playerState = StDashing; //Sets Dash state to true
        }

        private void Climb(KeyboardState keystate)
        {
            velocity.X = 0; //Player cannot move horizontally whilst climbing
            if (keystate.IsKeyDown(UpMoveKey)) //Moves player up
            {
                velocity.Y = -ClimbSpeed;
            }
            else if (keystate.IsKeyDown(DownMoveKey)) //Moves player down
            {
                velocity.Y = 2 * ClimbSpeed;
            }
            else
            {
                velocity.Y = 0; //Keeps player stationary
            }
        }

        #endregion

        #region Player Animation

        ///--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------        PLAYER ANIMATION CODE
        private void Animation(GameTime gameTime)
        {
            UpdateAnimState();
            if (prev_animState != animState || SpriteList.Count == 0) //Checks if player is in a different action state
            {
                FrameCount = 0;
                SwitchTextures(); //Lists frame dimensions into SpriteList
            }

            float dt = gameTime.ElapsedGameTime.Milliseconds; //Adds to FrameTimer
            FrameTimer += dt / 1000;

            if (FrameTimer > MaxFrameTime) //Checks if FrameTime is over
            {
                if (animState == AnimJumping)
                {
                    FrameCount = GetJumpAnimFrame();
                }
                else if (animState == AnimClimbing)
                {
                    FrameTimer = 0;
                    FrameCount = velocity.Y < 0 && FrameCount < FrameNum - 1 ? FrameCount + 1 : 0; //If the player is climbing, animates as normal, otherwise it sets the frame to the first frame ( for idle/ slliding )
                }
                else
                {
                    FrameTimer = 0;
                    FrameCount = FrameCount + 1 >= FrameNum ? 0 : FrameCount + 1; //If the FrameCount has hit the max frame amount, it is reset to 0
                }
            }

            current_frame = SpriteList[FrameCount]; //Sets current_frame accordingly
            prev_animState = animState; //Sets prev_animState to playerState to be used in the next frame
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void UpdateAnimState()
        {
            //Gets Animation State
            if (playerState == StClimbing) //Climbing Animation
            {
                animState = AnimClimbing;
            }
            else if (playerState == StNormal && velocity == Vector2.Zero && IsGrounded) //Idle Animation
            {
                animState = AnimIdle;
            }
            else if (playerState == StNormal && velocity != Vector2.Zero && IsGrounded) //Running Animation
            {
                animState = AnimRunning;
            }
            else if (playerState == StDashing) //Dashing Animation
            {
                animState = AnimDashing;
            }
            else if (!IsGrounded) //Jumping Animation
            {
                animState = AnimJumping;
            }
            else
            {
                animState = AnimRunning;
            }

            //Gets Direction
            if (velocity.X < 0) //When moving left
            {
                SpriteFlipper = SpriteEffects.FlipHorizontally; //Flips sprite to face left
            }
            else if (velocity.X > 0) //When moving right
            {
                SpriteFlipper = SpriteEffects.None; //Flips sprite to face right
            }

            //When neither are happening ( velocity.X == 0 ), SpriteFlipper remains unchanged, leaving the player facing the same direction.
        } ///UPDATES THE ANIMATION STATE OF THE PLAYER

        private void SwitchTextures()
        {
            GetSpriteSheet();
            SpriteList.Clear();
            int frameWidth = current_spritesheet.Width / FrameNum; //Finds frame dimensions
            int frameHeight = current_spritesheet.Height;
            for (int i = 0; i < FrameNum; i++) //Adds dimensions to list
            {
                SpriteList.Add(new Rectangle(i * frameWidth, 0, frameWidth, frameHeight));
            }
        } ///SWITCHES THE SPRITESHEET TEXTURE2D TO THE APPROPRIATE SPRITESHEET, REWRITES SPRITELIST ACCORDINGLLY

        private void GetSpriteSheet()
        {
            switch (animState)
            {
                case AnimIdle:
                    current_spritesheet = Content.Load<Texture2D>("IdleSheet"); //Idle animation
                    FrameNum = 9; //Number of Frames in the spritesheet
                    MaxFrameTime = 0.1111f; //Time per frame
                    break;
                case AnimRunning:
                    current_spritesheet = Content.Load<Texture2D>("RunSheet"); //Run Animation
                    FrameNum = 12; //Same as above
                    MaxFrameTime = 0.04f;
                    break;
                case AnimJumping:
                    current_spritesheet = Content.Load<Texture2D>("JumpSheet"); //Jump animation
                    FrameNum = 4; //THERE IS NO FRAME TIME, JUMP FRAMES ARE DETERMINED BY VERTICAL VELOCITY
                    break;
                case AnimDashing:
                    current_spritesheet = Content.Load<Texture2D>("DashSheet"); //Dashing SpriteSheet
                    FrameNum = 4;
                    MaxFrameTime = MaxDashTime / FrameNum;
                    break;
                case AnimClimbing:
                    current_spritesheet = Content.Load<Texture2D>("ClimbSheet"); //Climb animation
                    FrameNum = 6;
                    MaxFrameTime = 0.09f;
                    break;
                default:
                    break;
            }
        } ///RETURNS THE CORRECT SPRITESHEET

        private int GetJumpAnimFrame()
        {
            if (velocity.Y < -8) //Checks if in the first phase of jump
            {
                return 0;
            }
            else if (velocity.Y < 0) //Checks if player is nearing jump peak
            {
                return 1;
            }
            else if (velocity.Y < 4) //Checks if the player is just after the peak of the jump
            {
                return 2;
            }
            else if (velocity.Y > 4) //Checks if player is at the end of the jump
            {
                return 3;
            }

            return 4; ///This value here should never be returned, so I've set it extremely high to forcibly cause a runtime error to detect other errors
        } ///CALCULATES AND UPDATES THE JUMP FRAME

        #endregion

        public void LoadControls()
        {
            if (File.Exists("Keybinds.txt"))
            {
                var lines = File.ReadAllLines("Keybinds.txt");
                if (lines.Length >= 7)
                {
                    UpMoveKey = (Keys)int.Parse(lines[0]);
                    LeftMoveKey = (Keys)int.Parse(lines[1]);
                    DownMoveKey = (Keys)int.Parse(lines[2]);
                    RightMoveKey = (Keys)int.Parse(lines[3]);
                    JumpKey = (Keys)int.Parse(lines[4]);
                    DashKey = (Keys)int.Parse(lines[5]);
                    ClimbKey = (Keys)int.Parse(lines[6]);
                    return;
                }
            }

            UpMoveKey = Keys.W;
            LeftMoveKey = Keys.A;
            DownMoveKey = Keys.S;
            RightMoveKey = Keys.D;
            JumpKey = Keys.Space;
            DashKey = Keys.LeftShift;
            ClimbKey = Keys.LeftControl;
        }

        public void SaveControls()
        {
            string[] keybindIds =
            {
                ((int)UpMoveKey).ToString(),
                ((int)LeftMoveKey).ToString(),
                ((int)DownMoveKey).ToString(),
                ((int)RightMoveKey).ToString(),
                ((int)JumpKey).ToString(),
                ((int)DashKey).ToString(),
                ((int)ClimbKey).ToString()
            };

            File.WriteAllLines("Keybinds.txt", keybindIds);
        }

        private void LoadSounds()
        {
            // No-op placeholder for sound assets, kept to match the original code structure.
        }

        private void UpdateSound()
        {
            // No-op placeholder for sound updates.
        }

        private void GetPlayerIsGrounded()
        {
            IsGrounded = Position.Y >= 0 && Math.Abs(velocity.Y) < 0.5f;
        }

        private void GetPlayerIsColliding()
        {
            Hitbox.ResetCollisionVars();
            if (Position.X < 0)
            {
                Hitbox.IsCollidingLeft = true;
            }
            if (Position.X > Engine.Map.currentLevel.Width)
            {
                Hitbox.IsCollidingRight = true;
            }
        }

        private void CheckAllVerticies()
        {
            Hitbox.ResetCollisionVars();
        }
    }
}
