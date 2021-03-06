﻿namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        private delegate SDL_Renderer SDL_CreateRenderer_t(SDL_Window window, int index, uint flags);
        private static SDL_CreateRenderer_t s_sdl_createRenderer = LoadFunction<SDL_CreateRenderer_t>("SDL_CreateRenderer");
        public static SDL_Renderer SDL_CreateRenderer(SDL_Window window, int index, uint flags)
           => s_sdl_createRenderer(window, index, flags);

        private delegate void SDL_DestroyRenderer_t(SDL_Renderer renderer);
        private static SDL_DestroyRenderer_t s_sdl_destroyRenderer = LoadFunction<SDL_DestroyRenderer_t>("SDL_DestroyRenderer");
        public static void SDL_DestroyRenderer(SDL_Renderer renderer)
           => s_sdl_destroyRenderer(renderer);

        private delegate int SDL_SetRenderDrawColor_t(SDL_Renderer renderer, byte r, byte g, byte b, byte a);
        private static SDL_SetRenderDrawColor_t s_sdl_setRenderDrawColor
            = LoadFunction<SDL_SetRenderDrawColor_t>("SDL_SetRenderDrawColor");
        public static int SDL_SetRenderDrawColor(SDL_Renderer renderer, byte r, byte g, byte b, byte a)
            => s_sdl_setRenderDrawColor(renderer, r, g, b, a);

        private delegate int SDL_RenderClear_t(SDL_Renderer renderer);
        private static SDL_RenderClear_t s_sdl_renderClear = LoadFunction<SDL_RenderClear_t>("SDL_RenderClear");
        public static int SDL_RenderClear(SDL_Renderer renderer) => s_sdl_renderClear(renderer);

        private delegate int SDL_RenderFillRect_t(SDL_Renderer renderer, void* rect);
        private static SDL_RenderFillRect_t s_sdl_renderFillRect = LoadFunction<SDL_RenderFillRect_t>("SDL_RenderFillRect");
        public static int SDL_RenderFillRect(SDL_Renderer renderer, void* rect) => s_sdl_renderFillRect(renderer, rect);

        private delegate int SDL_RenderPresent_t(SDL_Renderer renderer);
        private static SDL_RenderPresent_t s_sdl_renderPresent = LoadFunction<SDL_RenderPresent_t>("SDL_RenderPresent");
        public static int SDL_RenderPresent(SDL_Renderer renderer) => s_sdl_renderPresent(renderer);

    }
}
