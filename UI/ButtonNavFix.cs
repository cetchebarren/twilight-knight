using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonNavFix : Button
{
    // partially override the automatic navigation by setting just some of these
    public Selectable selectOverrideOnUp;
    public Selectable selectOverrideOnDown;
    public Selectable selectOverrideOnLeft;
    public Selectable selectOverrideOnRight;

    /// <summary>
    /// Leave navigation on Automatic then override just one or some directions with explicit targets.
    /// </summary>
    public override Selectable FindSelectableOnUp()
    {
        return selectOverrideOnUp != null && selectOverrideOnUp.gameObject.activeSelf
            ? selectOverrideOnUp : base.FindSelectableOnUp();
    }
    public override Selectable FindSelectableOnDown()
    {
        return selectOverrideOnDown != null && selectOverrideOnDown.gameObject.activeSelf
            ? selectOverrideOnDown : base.FindSelectableOnDown();
    }
    public override Selectable FindSelectableOnLeft()
    {
        return selectOverrideOnLeft != null && selectOverrideOnLeft.gameObject.activeSelf
            ? selectOverrideOnLeft : base.FindSelectableOnLeft();
    }
    public override Selectable FindSelectableOnRight()
    {
        return selectOverrideOnRight != null && selectOverrideOnRight.gameObject.activeSelf
            ? selectOverrideOnRight : base.FindSelectableOnRight();
    }
}