import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [],
  optimizeDeps:{},
  build: {
    outDir: "dist"
  },
  base: "/fsadvent2023/"
})
