const GRID_SIZE = 25; // 5x5px grid


function bindElementToNodeProperty(elementId, node, propertyName) {
  let element = document.getElementById(elementId);
  element.outerHTML = element.outerHTML;//reset listeners
  element = document.getElementById(elementId);
  element.value = node[propertyName];
  element.onblur = function (event) {
    node[propertyName] = event.target.value;
  };
}


function getTextWidth(text, font) {
  const canvas = document.createElement('Canvas');
  const context = canvas.getContext('2d');
  context.font = font;
  return context.measureText(text).width;
}
function snapToGrid(value) {
  return Math.round(value / GRID_SIZE) * GRID_SIZE;
}

function CreateNode(id, text, description, status, executionDuration, x, y, onSelect, onDrop) {
  const canvas = document.getElementById("bpnCanvas");
  const node = document.createElement("div");
  node.className = "node";
  node.id = id;
  node.style.left = x + "px";
  node.style.top = y + "px";

  if (status === 'Succeeded') {
    node.style.boxShadow = "0 0 10px 2px green";
  } else if (status === 'Failed') {
    node.style.boxShadow = "0 0 10px 2px red";
  }

  const header = document.createElement("div");
  header.className = "node-header"; 
  header.innerText = text;
  node.appendChild(header);

  const desc = document.createElement("div");
  desc.className = "node-description";
  desc.innerText = description;
  node.appendChild(desc);

  if (executionDuration) {
    const duration = document.createElement("div");
    duration.className = "node-duration";
    duration.innerText = `Execution Time: ${executionDuration}`;
    duration.style.fontSize = "10px";
    duration.style.color = "lightgrey";
    node.appendChild(duration);
  }


  const edit = document.createElement("div");
  edit.className = "node-edit"; 
  const icon = document.createElement("i");
  icon.className = "bi bi-pencil"; 
  edit.appendChild(icon);
  node.appendChild(edit);

  edit.addEventListener("click", (e) => {
    onSelect && onSelect(id); // Trigger the callback with node id
  });

  canvas.appendChild(node);

  let startX, startY;
  node.addEventListener("mousedown", dragStart);

  function dragStart(e) {
    e.preventDefault();
    startX = e.clientX;
    startY = e.clientY;
    const offsetX = e.clientX - node.offsetLeft;
    const offsetY = e.clientY - node.offsetTop;

    function dragMove(e) {
      const dx = e.clientX - startX;
      const dy = e.clientY - startY;

      node.style.left = (e.clientX - offsetX) + "px";
      node.style.top = (e.clientY - offsetY) + "px";
    }

    function dragEnd(e) {
      document.removeEventListener("mousemove", dragMove);
      document.removeEventListener("mouseup", dragEnd);

      node.style.left = snapToGrid(e.clientX - offsetX) + "px";
      node.style.top = snapToGrid(e.clientY - offsetY) + "px";


      onDrop && onDrop(id, snapToGrid(e.clientX - offsetX), snapToGrid(e.clientY - offsetY));

    }

    document.addEventListener("mousemove", dragMove);
    document.addEventListener("mouseup", dragEnd);
  }
}

function clearConnectedNodes() {
  const svg = document.getElementById("connectorLayer");
  const defs = svg.querySelector('defs');
  svg.innerHTML = "";
  svg.appendChild(defs);
}
function connectNodes(node1, node2, label) {
  const svg = document.getElementById("connectorLayer");
  const line = document.createElementNS("http://www.w3.org/2000/svg", "line");

  const rect1 = node1.getBoundingClientRect();
  const rect2 = node2.getBoundingClientRect();

  const node1X = rect1.left + window.scrollX + (rect1.width);
  const node1Y = rect1.top + window.scrollY + (rect1.height / 2);
  const node2X = rect2.left + window.scrollX;
  const node2Y = rect2.top + window.scrollY + (rect2.height / 2);

  line.setAttribute("x1", node1X);
  line.setAttribute("y1", node1Y);
  line.setAttribute("x2", node2X);
  line.setAttribute("y2", node2Y);
  line.setAttribute("stroke", "black");
  line.setAttribute("stroke-width", "2");
  line.setAttribute("marker-end", "url(#arrowhead)");
  svg.appendChild(line);

  const midX = (rect1.left + rect1.width / 2 + rect2.left + rect2.width / 2) / 2 + window.scrollX;
  const midY = (rect1.top + rect1.height / 2 + rect2.top + rect2.height / 2) / 2 + window.scrollY;
  const rectWidth = getTextWidth(label, "16px Arial, sans-serif") + 10;
  const rect = document.createElementNS("http://www.w3.org/2000/svg", "rect");
  rect.setAttribute("x", midX - (rectWidth / 2)); 
  rect.setAttribute("y", midY - 10); 
  rect.setAttribute("width", rectWidth); 
  rect.setAttribute("height", 20); 
  rect.setAttribute("fill", "white"); 
  rect.setAttribute("stroke", "none"); 
  svg.appendChild(rect);

  const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
  text.setAttribute("x", midX);
  text.setAttribute("y", midY);
  text.setAttribute("text-anchor", "middle");
  text.setAttribute("dominant-baseline", "middle");
  text.setAttribute("fill", "black");
  text.setAttribute("font-size", "12px");
  text.textContent = label;
  svg.appendChild(text);
}

  
