local heater = {}

heater.name = "PrismaticHelper/Heater"
heater.fillColor = {0.7, 0.2, 0.1, 0.6}
heater.borderColor = {0.7, 0.2, 0.1, 0.4}
heater.placements = {
    name = "heater",
    data = {
        width = 8,
        height = 8,
        maxTime = 7
    }
}

local freezer = {}

freezer.name = "PrismaticHelper/Freezer"
freezer.fillColor = {0.1, 0.2, 0.7, 0.6}
freezer.borderColor = {0.1, 0.2, 0.7, 0.4}
freezer.placements = {
    name = "freezer",
    data = {
        width = 8,
        height = 8,
        maxTime = 7
    }
}

return { heater, freezer }