shader_type canvas_item;

// Le centre est maintenant en UV (0.5, 0.5 est le milieu du node)
uniform vec2 center = vec2(0.5, 0.5);
uniform float radius = 0.0;
uniform float feather = 0.2;
uniform vec4 base_color : source_color;
uniform vec4 target_color : source_color;

void fragment() {
    // On récupère la couleur de la texture (les nuages qui tournent)
    vec4 tex_color = texture(TEXTURE, UV);
    
    // On calcule la distance basée sur l'UV local (0.0 à 1.0)
    // Comme l'UV tourne avec le node, l'onde "suit" la rotation
    float dist = distance(UV, center);
    
    // Masque d'onde
    float visibility = smoothstep(0.0, 0.05, radius);
    float mask = smoothstep(radius, radius - feather, dist) * visibility;
    
    vec4 final_tint = mix(base_color, target_color, mask);
    
    // Application de la couleur
    vec3 blended_rgb = mix(tex_color.rgb, tex_color.rgb * final_tint.rgb, final_tint.a);
    
    COLOR = vec4(blended_rgb, tex_color.a);
}