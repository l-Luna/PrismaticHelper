local rotatingParallax = {}

rotatingParallax.name = "PrismaticHelper/RotatingParallax"
rotatingParallax.canBackground = true
rotatingParallax.canForeground = true

rotatingParallax.defaultData = {
    x = 0, y = 0, scrollX = 0, scrollY = 0, speedX = 0, speedY = 0,
    
    texture = "",
    atlas = "game",
    alpha = 1,
    rotationSpeed = 1,
    scale = 1,
    fadeIn = false
}

return rotatingParallax