local utils = require("utils")

local amalgam = {}

amalgam.name = "PrismaticHelper/AmalgamatedCassetteBlock"
amalgam.minimumSize = {16, 16}
amalgam.placements = {
    name = "amalgam",
    data = {
        indices = "0,1,2,3",
        colors = "49aaf0,f049be,fcdc3a,38e04e"
    }
}

function amalgam.fillColor(_, entity)
    local colors = (entity.colors and string.len(entity.colors) >= 6 and entity.colors) or "000000"
    return utils.getColor(string.sub(colors, 1, 6))
end 

return amalgam