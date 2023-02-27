-- copied from loenn's built-in kevin plugin

local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local kevin = {}

local axesOptions = {
    Both = "both",
    Vertical = "vertical",
    Horizontal = "horizontal"
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

local function mul(l, r)
    return { l[1] * r[1], l[2] * r[2], l[3] * r[3] }
end

kevin.name = "PrismaticHelper/CassetteKevin"
kevin.depth = 0
kevin.minimumSize = {24, 24}
kevin.fieldInformation = {
    axes = {
        options = axesOptions,
        editable = false
    },
    index = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    }
}
kevin.placements = {}

for _, axis in pairs(axesOptions) do
    table.insert(kevin.placements, {
        name = axis,
        data = {
            width = 24,
            height = 24,
            axes = axis,
            chillout = false,
            index = 0
        }
    })
end

local frameTextures = {
    none = "PrismaticHelper/cassetteKevin/block00",
    horizontal = "PrismaticHelper/cassetteKevin/block01",
    vertical = "PrismaticHelper/cassetteKevin/block02",
    both = "PrismaticHelper/cassetteKevin/block03"
}

local ninePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}

local kevinColor = {54 / 255, 54 / 255, 54 / 255}
local smallFaceTexture = "PrismaticHelper/cassetteKevin/idle_face"
local giantFaceTexture = "PrismaticHelper/cassetteKevin/giant_block00"

function kevin.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local axes = entity.axes or "both"
    local chillout = entity.chillout
    
    local index = entity.index or 0
    local color = colors[index + 1] or colors[1]

    local giant = height >= 48 and width >= 48 and chillout
    local faceTexture = giant and giantFaceTexture or smallFaceTexture

    local frameTexture = frameTextures[axes] or frameTextures["both"]
    local ninePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)
    ninePatch:setColor(color)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, mul(kevinColor, color))
    local faceSprite = drawableSprite.fromTexture(faceTexture, entity)
    faceSprite:setColor(color)

    faceSprite:addPosition(math.floor(width / 2), math.floor(height / 2))

    local sprites = ninePatch:getDrawableSprite()

    table.insert(sprites, 1, rectangle:getDrawableSprite())
    table.insert(sprites, 2, faceSprite)

    return sprites
end

function kevin.rotate(room, entity, direction)
    local axes = (entity.axes or ""):lower()

    if axes == "horizontal" then
        entity.axes = "vertical"

    elseif axes == "vertical" then
        entity.axes = "horizontal"
    end

    return true
end

return kevin