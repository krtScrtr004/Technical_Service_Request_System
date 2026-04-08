particlesJS("particles_js", {
    particles: {
        number: { 
            value: 80, 
            density: { 
                enable: true, 
                value_area: 800
            }
        },
        color: { value: ["#1976D2", "#00ACC1"] },
        shape: { 
            type: "circle", 
            stroke: { 
                width: 0, 
                color: "#000000"
            }
        },
        opacity: { 
            value: 0.5, 
            random: true
        },
        size: { 
            value: 3, 
            random: true
        },
        line_linked: {
            enable: true,
            distance: 120,
            color: "#1976D2",
            opacity: 0.4,
            width: 1
        },
        move: { 
            enable: true, 
            speed: 2, 
            direction: "none", 
            random: false, 
            straight: false, 
            out_mode: "out"
        }
    },
    interactivity: {
        detect_on: "canvas",
        events: {
            onhover: { enable: true, mode: "grab" },
            onclick: { enable: true, mode: "push" },
            resize: true
        },
        modes: {
            grab: { distance: 140, line_linked: { opacity: 0.7 } },
            push: { particles_nb: 4 }
        }
    },
    retina_detect: true
});

document.querySelector("#login_button").addEventListener("click", function(e) {
    const button = e.currentTarget;

    const circle = document.createElement("span");
    const diameter = Math.max(button.clientWidth, button.clientHeight);
    const radius = diameter / 2;

    circle.style.width = circle.style.height = `${diameter}px`;
    circle.style.left = `${e.clientX - button.getBoundingClientRect().left - radius}px`;
    circle.style.top = `${e.clientY - button.getBoundingClientRect().top - radius}px`;
    circle.classList.add("ripple");

    const ripple = button.getElementsByClassName("ripple")[0];
    if (ripple) {
        ripple.remove();
    }

    button.appendChild(circle);
});