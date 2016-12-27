
namespace arookas {

	public class bloBlendPane : bloBoundPane {

		protected double mBlendTime, mBlendStep;
		protected bool mBlendActive;

		public bloBlendPane(bloPane pane)
			: base(pane) {
			initialize();
		}
		public bloBlendPane(bloScreen screen, uint name)
			: base(screen, name) {
			initialize();
		}

		void initialize() {
			mBlendActive = false;
			mBlendStep = 0.0d;
			mBlendTime = 0.0d;
		}

		public override bool update() {
			bool result = base.update();
			if (mBlendActive) {
				if (mBlendTime >= 1.0d) {
					mBlendActive = false;
				}
				var picture = (mPane as bloPicture);
				picture.setBlendFactor(mBlendTime, 0);
				picture.setBlendFactor((1.0d - mBlendTime), 1);
				picture.setBlendFactor(1.0d, 2);
				picture.setBlendFactor(1.0d, 3);
				picture.setBlendKonstColor();
				picture.setBlendKonstAlpha();
				mBlendTime += mBlendStep;
			}
			return (result && !mBlendActive);
		}

		public void setPaneBlend(int steps, bloTexture to, bloTexture from) {
			var picture = (mPane as bloPicture);
			if (picture != null) {
				if (from != null) {
					picture.changeTexture(to, 0);
					picture.changeTexture(from, 1);
				} else {
					picture.changeTexture(to, 0);
				}
				mBlendActive = true;
				mBlendStep = (1.0d / steps);
				mBlendTime = 0.0d;
			}
		}

	}

}
