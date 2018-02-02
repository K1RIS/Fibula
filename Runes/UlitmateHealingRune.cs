public class UlitmateHealingRune : HealingRune {

    override protected int heal
    {
        get
        {
            return 100;
        }
    }

    override public void use(Player player)
    {
        base.use(player);
    }
}
