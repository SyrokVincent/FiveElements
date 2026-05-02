shader_type canvas_item;

// Le centre est maintenant en UV (0.5, 0.5 est le milieu du node)
uniform vec2 center = vec2(0.5, 0.5);
uniform float radius = 0.0;
uniform float feather = 0.2;
uniform vec4 base_color : source_color;
uniform vec4 target_color : source_color;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    float dist = distance(UV, center);
    
    // 1. Calcul du masque de l'onde
    float visibility = smoothstep(0.0, 0.05, radius);
    float mask = smoothstep(radius, radius - feather, dist) * visibility;
    
    // 2. Déterminer la couleur de la teinte actuelle
    // On transitionne de base_color (l'ancienne) vers target_color (la nouvelle)
    vec4 current_tint = mix(base_color, target_color, mask);
    
    // 3. Application du mode MULTIPLY
    // On multiplie la texture par notre teinte calculée
    vec3 multiplied = tex_color.rgb * current_tint.rgb;
    
    // 4. Mixage final basé sur l'alpha de la teinte
    // Cela permet de ne pas multiplier si l'alpha de tes couleurs est à 0
    vec3 final_rgb = mix(tex_color.rgb, multiplied, current_tint.a);
    
    COLOR = vec4(final_rgb, tex_color.a);
}