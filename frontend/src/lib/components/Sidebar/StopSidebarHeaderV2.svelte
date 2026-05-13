<script lang="ts">
  const { title, id } = $props()
</script>

<div class="sticky top-0 z-10 bg-white">
  <p class="text-xl font-bold">{title}</p>
  <p class="text-xs font-light">{id}</p>
</div>