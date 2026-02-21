// PixelationRenderFeature.cs

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        [Range(1, 8)] public int downscaleFactor = 3;
        // 1 = sin efecto, 2 = mitad res, 3 = un tercio, etc.
    }

    public Settings settings = new Settings();
    private PixelationPass pass;

    public override void Create()
    {
        pass = new PixelationPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }

    private class PixelationPass : ScriptableRenderPass
    {
        private int downscale;
        private RTHandle lowResRT;

        public PixelationPass(Settings settings)
        {
            this.downscale = settings.downscaleFactor;
            this.renderPassEvent = settings.renderPassEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.width /= downscale;
            desc.height /= downscale;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref lowResRT, desc,
                FilterMode.Point, // CRUCIAL: Point filtering = pixeles duros
                name: "_LowResRT");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Pixelation");

            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // Baja resolución
            Blit(cmd, source, lowResRT);
            // Vuelta a resolución original (con Point filter = pixeles)
            Blit(cmd, lowResRT, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}