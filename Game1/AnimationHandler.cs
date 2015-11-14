using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
namespace TheGame
{
	class AnimationHandler
	{
		private List<int> frames;
		private List<int> durations;
        private List<bool> loops;
		private int currentFrame;
		private double t;
		private int spriteWidth;
		private int spriteHeight;

		public int currentAnimation;
		public Rectangle textureSection;

        private NetworkInterface network;

        private bool ignoreAnimationChange;

		public AnimationHandler (int height, int width, List<int> frameCounts, List<int> frameDurations, List<bool> willLoop)
		{
            ignoreAnimationChange = false;
			spriteHeight = height;
			spriteWidth = width;
			frames = frameCounts;
			durations = frameDurations;
			textureSection = new Rectangle (0, 0, width, height);
			t = 0;
            loops = willLoop;
            network = null;
		}

		public int animationCount()
		{
			return durations.Count;
		}

        public void lockAnimation()
        {
            ignoreAnimationChange = true;
        }
        public void unlockAnimation()
        {
            ignoreAnimationChange = false;
        }

        public void doNetworkedAnimationUpdates(NetworkInterface ni)
        {
            network = ni;
        }

		public void setAnimation(int n, bool resetTime)
		{
			if (n < 0 || n > this.animationCount () - 1 || ignoreAnimationChange)
				return;
			currentAnimation = n;
			textureSection = new Rectangle (currentFrame * spriteWidth, spriteHeight * n, spriteWidth, spriteHeight);
			if (resetTime) {
				t = 0;
				currentFrame = 0;
                textureSection = new Rectangle(0, spriteHeight * n, spriteWidth, spriteHeight);
			} else if (currentFrame >= frames [currentAnimation]) {
				currentFrame = 0;
                textureSection = new Rectangle(0, spriteHeight * n, spriteWidth, spriteHeight);
			}
            if (network != null && network.mapLoaded)
            {
                network.sendAnimationUpdate(this);
            }
		}

		public void update(double dt)
		{
			if (animationCount() == 0 || (currentFrame >= frames[currentAnimation] -1 && !loops[currentAnimation])) {
				return;
			}
			int nextFrameT = (currentFrame + 1) * durations [currentAnimation];
			if (t + dt >= nextFrameT)
			{	
				currentFrame++;
				textureSection = new Rectangle (textureSection.X + spriteWidth, textureSection.Y, spriteWidth, spriteHeight);
			}

			if (currentFrame > frames [currentAnimation] - 1 && loops[currentAnimation])
			{
				currentFrame = 0;
				textureSection = new Rectangle (0, textureSection.Y, spriteWidth, spriteHeight);
				t = 0;
			}

			t += dt;

		}


	}
}

