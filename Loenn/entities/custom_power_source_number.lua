local powerSourceNum = {}

powerSourceNum.name = "PrismaticHelper/CustomPowerSourceNumber"
powerSourceNum.placements = {
    name = "custom_power_source_number",
    data = {
        cond = "1:1",
        base = "scenery/powersource_numbers/1",
        glow = "scenery/powersource_numbers/1_glow",
        requiresLightningDisabled = true
    }
}
powerSourceNum.texture = "scenery/powersource_numbers/1"
powerSourceNum.justify = {0.5, 0.5}

return powerSourceNum