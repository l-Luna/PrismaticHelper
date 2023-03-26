local drawableSprite = require("structs.drawable_sprite")

local boombox = {}

local dirOptions = {
    None = "",
    Up = "up",
    Down = "down",
    Left = "left",
    Right = "right"
}

local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

local colors = {
    {73 / 255, 170 / 255, 240 / 255},
    {240 / 255, 73 / 255, 190 / 255},
    {252 / 255, 220 / 255, 58 / 255},
    {56 / 255, 224 / 255, 78 / 255}
}

boombox.name = "PrismaticHelper/Boombox"
boombox.placements = {
    name = "boombox",
    data = {
        index = 0,
        big = false,
        attached = false,
        direction = ""
    }
}
boombox.fieldInformation = {
    direction = {
        options = dirOptions,
        editable = false
    },
    index = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    }
}

local texBase = "PrismaticHelper/boombox/solid"

function boombox.sprite(room, entity)
    local index = entity.index or 0
    local color = colors[index + 1] or colors[1]

    local sprite = drawableSprite.fromTexture(texBase .. (entity.big and "_big0" or "0") .. index, entity)
    sprite:setColor(color)
    sprite:setJustification(0, 0)
    
    return { sprite }
end

return boombox