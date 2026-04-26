shader_type canvas_item;

// On a besoin de la position et de la taille de "Layers" sur l'écran
uniform vec2 parent_screen_pos;
uniform vec2 parent_size;

uniform vec2 center = vec2(0.5, 0.5);
uniform float radius : hint_range(0.0, 2.0) = 0.0;
uniform float feather : hint_range(0.0, 1.0) = 0.1;

uniform vec4 base_color : source_color = vec4(0.0, 0.0, 0.0, 0.0);
uniform vec4 target_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void fragment() {
    // 1. On récupère la texture de l'enfant (orbe, flèche, etc.)
    vec4 tex_color = texture(TEXTURE, UV);
    
    // 2. CALCUL MAGIQUE : On crée des "UV Globaux"
    // On prend la position du pixel sur l'écran, on soustrait le coin de "Layers"
    // et on divise par sa taille. On obtient un UV de 0 à 1 pour TOUT le groupe.
    vec2 global_uv = (FRAGCOORD.xy - parent_screen_pos) / parent_size;
    
    // 3. Le reste du calcul utilise global_uv au lieu de UV
    float dist = distance(global_uv, center);
    float cutoff = radius * (1.1); // Petite marge pour le feather
    float mask = smoothstep(cutoff, cutoff - feather, dist);
    
    vec4 final_tint = mix(base_color, target_color, mask);
    vec3 blended_rgb = mix(tex_color.rgb, tex_color.rgb * final_tint.rgb, final_tint.a);
    
    COLOR = vec4(blended_rgb, tex_color.a);
}