using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public Rigidbody2D rigid;
    public BoxCollider2D headCollider;
    public CircleCollider2D footCollider;
    public LineRenderer hook;
    public SpriteRenderer sprite;

    [Header("Shared variables")]
    float horizontalMove = 0f;

    [Header("Bool Actions")]
    public bool isMove = false;
    public bool isJump = false;
    public bool isCrouch = false;
    public bool isDash = false;
    public bool isPlane = false;
    public bool isClimb = false;
    public bool isSwim = false;
    public bool isHook = false;
    public bool isShootMagic = false;
    public bool isShootBow = false;
    public bool isDamage = false;

    [Header("Inputs")]
    float horizontalAxis;
    float verticalAxis;

    bool jumpDown;
    bool crouchDown;
    bool crouchUp;
    bool DashDown;
    bool planeDown;
    bool planePressed;
    bool climbPressed;
    // Swim
    bool hookDown;
    bool shootMagicDown;
    bool shootBowDown;

    [Header("Checks")]
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;
    public LayerMask whatIsWater;
    public LayerMask whatIsGrapplable;
    public LayerMask whatIsDamage;
    public Transform groundCheck;
    public Transform ceilingCheck;
    public Transform wallCheck;
    [Space]
    public float groundedRadius;
    public float walledRadius;
    public float wateredRadius;
    public bool grounded;
    public float ceilingRadius;
    public bool facingRight = true;
    public Vector3 velocityRef;

    [Header("Events")]
    public UnityEvent OnLandEvent;
    public UnityEvent<bool> OnCrouchEvent;
    private bool wasCrouching = false;

    [Space]

    [Header("Stats")]
    public int health;
    public int coins;
    public int level;

    public int meleeDamage;
    public int magicDamage;
    public int meleeDefense;
    public int magicDefense;

    [Header("Move")]
    public float runSpeed;
    public int gravityScale;
    [Range(0, 0.3f)] public float timeMoveSmoothing;
    public bool airControl;

    [Header("Jump")]
    public float jumpForce;
    public int maxJumps;
    public int jumpsLeft;

    [Header("Crouch")]
    public Collider2D crouchDisableCollider;
    [Range(0, 1)] public float crouchVelReduction;

    [Header("Dash")]
    public float timeDash;
    public float forceDash;
    public bool canDash = true;

    [Header("Plane")]
    [Range(0, 1)] public float planeGravityReduction;

    [Header("Climb")]
    public float climbSpeed;

    [Header("Swim")]
    public float swimSpeed;

    [Header("Hook")]
    public float hookRange;
    public float hookSpeed;
    public float hookShootSpeed;
    public bool hookRetracting = false;
    public Vector2 target;

    [Header("Shoot Magic")]
    public GameObject bullet;
    public float bulletVel;

    public enum MagicType { Normal, Fire, Water, Electric, Grass }
    public MagicType magicType = MagicType.Normal;

    [Header("Shoot Bow")]
    public GameObject bow;
    public GameObject arrow;
    public float arrowForce;
    public Transform shotPoint;
    public int comboCount;
    public int maxComboCount;
    public float comboTimer;
    public float maxComboTimer;
    public bool startComboTimer;
    public float delayShootBow;
    public float counterDelayShootBow;

    [Header("Damage")]
    public float timeDamage;

    [Header("CheckPoints")]
    public Vector2 SpawnPoint;

    [Header("Melee")]
    public bool isAttacking;
    public bool isChargingAttack;

    #region MonoBehaviour methods
    void Awake()
    {
        // Components
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        headCollider = GetComponent<BoxCollider2D>();
        crouchDisableCollider = GetComponent<BoxCollider2D>();
        footCollider = GetComponent<CircleCollider2D>();
        hook = GetComponent<LineRenderer>();
        sprite = GetComponent<SpriteRenderer>();

        // Events
        if (OnLandEvent == null)
        {
            OnLandEvent = new UnityEvent();
        }

        if (OnCrouchEvent == null)
        {
            OnCrouchEvent = new UnityEvent<bool>();
        }

        // Variables
        maxJumps--;
        jumpsLeft = maxJumps;

        counterDelayShootBow = delayShootBow;

        meleeDamage = level * 10;
        magicDamage = level * 10;
        meleeDefense = level * 10;
        magicDefense = level * 10;
    }

    void Update()
    {
        GetInputs();
        Jump();
        Crouch();
        Dash();
        Plane();
        Climb();
        Hook();
        Swim();
        ShootMagic();
        ShootBow();
        Checks();
        Melee();
    }

    void FixedUpdate()
    {
        Move();
    }
    #endregion

    #region My methods
    public void GetInputs()
    {
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        jumpDown = Input.GetButtonDown("Jump");
        crouchDown = Input.GetButtonDown("Crouch");
        crouchUp = Input.GetButtonUp("Crouch");
        DashDown = Input.GetButtonDown("Dash");
        planeDown = Input.GetButtonDown("Plane");
        planePressed = Input.GetButton("Plane");
        climbPressed = Input.GetButton("Climb");
        hookDown = Input.GetButtonDown("Fire2");
        shootMagicDown = Input.GetButtonDown("Fire1");
        shootBowDown = Input.GetButtonDown("Bow");
    }

    public void Move()
    {
        if (!isDash && !isClimb)
        {
            // Detectar suelo
            bool wasGrounded = grounded;
            grounded = false;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    grounded = true;
                    if (!wasGrounded)
                        OnLandEvent.Invoke();
                }
            }

            // Nos guardamos en una variable el movimiento (direccion + velocidad)
            horizontalMove = horizontalAxis * runSpeed;
            float move = horizontalMove * Time.fixedDeltaTime;
            animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

            // Nos guardamos en una variable si estamos agachados
            bool crouch = isCrouch;

            // Si estamos agachados vemos si nos podemos levantar
            if (!crouch)
            {
                // Para comprobarlo vemos si hay algo encima de nuestea cabeza usando el ceilingCheck
                if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
                {
                    crouch = true;
                }
            }

            // Nos podemos mover solo si el jugador está tocando el suelo o se permite controlarlo en el aire
            if (grounded || airControl)
            {
                // Si estamos agachados
                if (crouch)
                {
                    // Si en el frame anterior estabamos de pie decimos que estamos agachados e invocamos los eventos
                    if (!wasCrouching)
                    {
                        wasCrouching = true;
                        OnCrouchEvent.Invoke(true);
                    }

                    // Reducimos la velocidad
                    move *= crouchVelReduction;

                    // Desacticamos el collider de arriba
                    if (crouchDisableCollider != null)
                    {
                        crouchDisableCollider.enabled = false;
                    }
                }
                // Si estamos de pie
                else
                {
                    // Acticamos el collider de arriba
                    if (crouchDisableCollider != null)
                    {
                        crouchDisableCollider.enabled = true;
                    }

                    // Si en el frame anterior estabamos de agachados decimos que estamos de pié e invocamos los eventos
                    if (wasCrouching)
                    {
                        wasCrouching = false;
                        OnCrouchEvent.Invoke(false);
                    }
                }

                // Calculamos la nueva velocidad
                Vector3 targetVelocity = new Vector2(move * 10f, rigid.velocity.y);
                // Aplicamos la nueva velocidad
                rigid.velocity = Vector3.SmoothDamp(rigid.velocity, targetVelocity, ref velocityRef, timeMoveSmoothing);

                if (rigid.velocity == Vector2.zero)
                {
                    isMove = false;
                }
                else
                {
                    isMove = true;
                }

                // Hacemos flip al personaje
                if ((move > 0 && !facingRight) || (move < 0 && facingRight))
                {
                    Flip();
                }
            }

            //isJump = false;
        }
    }

    public void Flip()
    {
        // Decimos que hemos cambiado de lado
        facingRight = !facingRight;

        // Cambiamos de lado
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void Jump()
    {
        if (jumpDown && jumpsLeft > 0 && !isCrouch && !isDash && !isClimb && !isPlane)
        {
            jumpsLeft = Mathf.Max(0, jumpsLeft - 1);
            animator.SetBool("IsJumping", true);

            isJump = true;
            grounded = false;

            rigid.velocity = new Vector2(rigid.velocity.x, 0);
            rigid.AddForce(new Vector2(0f, jumpForce));
        }
    }

    public void Crouch()
    {
        if (crouchDown && !isJump && !isDash && !isPlane)
        {
            isCrouch = true;
        }
        else if (crouchUp)
        {
            isCrouch = false;
        }
    }

    public void Dash()
    {
        if (DashDown && !isDash && !isCrouch && canDash)
        {
            canDash = false;

            int face = 0;
            if (facingRight)
            {
                face = 1;
            }
            else
            {
                face = -1;
            }

            isDash = true;
            rigid.velocity = Vector2.zero;
            rigid.gravityScale = 0;
            rigid.AddForce(new Vector2(face * forceDash, 0.0f));

            Invoke("DashOut", timeDash);
        }
    }

    public void DashOut()
    {
        rigid.gravityScale = gravityScale;
        isDash = false;
    }

    public void Plane()
    {
        if (!isDash && !isClimb && !isCrouch && !isSwim)
        {
            if (planeDown)
            {
                rigid.velocity = new Vector2(rigid.velocity.x, 0.0f);
            }

            if (planePressed)
            {
                isPlane = true;
                if (!grounded)
                {
                    rigid.gravityScale = planeGravityReduction;
                }
            }
            else
            {
                isPlane = false;
                rigid.gravityScale = gravityScale;
            }
        }
    }

    public void Climb()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(wallCheck.position, walledRadius, whatIsWall);

        if (colliders.Length > 0 && climbPressed && !isSwim)
        {
            isClimb = true;
            rigid.gravityScale = 0;

            float horizontalSpeed = horizontalAxis * climbSpeed * Time.fixedDeltaTime;
            float verticalSpeed = verticalAxis * climbSpeed * Time.fixedDeltaTime;

            Vector3 targetVelocity = new Vector2(horizontalSpeed * 10f, verticalSpeed * 10f);
            rigid.velocity = Vector3.SmoothDamp(rigid.velocity, targetVelocity, ref velocityRef, timeMoveSmoothing);

            jumpsLeft = maxJumps;
        }
        else if (!isPlane)
        {
            isClimb = false;
            rigid.gravityScale = gravityScale;
        }
    }

    public void Swim()
    {
        if (isSwim)
        {
            float horizontalSpeed = horizontalAxis * climbSpeed * Time.fixedDeltaTime;
            float verticalSpeed = verticalAxis * climbSpeed * Time.fixedDeltaTime;

            Vector3 targetVelocity = new Vector2(horizontalSpeed * 10f, verticalSpeed * 10f);
            rigid.velocity = Vector3.SmoothDamp(rigid.velocity, targetVelocity, ref velocityRef, timeMoveSmoothing);
        }
    }

    public void Hook()
    {
        if (hookDown && !isHook)
        {
            Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, hookRange, whatIsGrapplable);

            if (hit.collider != null)
            {
                rigid.velocity = Vector2.zero;
                jumpsLeft = maxJumps;

                isHook = true;
                target = hit.point;
                hook.enabled = true;
                hook.positionCount = 2;

                StartCoroutine(Grapple());
            }
        }

        if (hookRetracting)
        {
            Vector2 grapplePos = Vector2.Lerp(transform.position, target, hookSpeed * Time.deltaTime);

            transform.position = grapplePos;

            hook.SetPosition(0, transform.position);

            if (Vector2.Distance(transform.position, target) < 1f)
            {
                hookRetracting = false;
                isHook = false;
                hook.enabled = false;

                rigid.gravityScale = 0;
                jumpsLeft = maxJumps;
            }
        }
    }

    IEnumerator Grapple()
    {
        float t = 0;
        float time = 10;

        hook.SetPosition(0, transform.position);
        hook.SetPosition(1, transform.position);

        Vector2 newPos;

        for (; t < time; t += hookShootSpeed * Time.deltaTime)
        {
            newPos = Vector2.Lerp(transform.position, target, t / time);
            hook.SetPosition(0, transform.position);
            hook.SetPosition(1, newPos);
            yield return null;
        }

        hook.SetPosition(1, target);
        hookRetracting = true;
    }

    public void ShootMagic()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            magicType = MagicType.Normal;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            magicType = MagicType.Grass;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            magicType = MagicType.Fire;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            magicType = MagicType.Water;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            magicType = MagicType.Electric;
        }

        if (shootMagicDown)
        {
            isShootMagic = true;

            GameObject bulletInst = Instantiate(bullet, transform.position, transform.rotation);
            Rigidbody2D bulletRigid = bulletInst.GetComponent<Rigidbody2D>();
            Bullet bulletScript = bulletInst.GetComponent<Bullet>();

            switch (magicType)
            {
                case MagicType.Normal:
                    bulletScript.type = Bullet.Type.Normal;
                    break;
                case MagicType.Grass:
                    bulletScript.type = Bullet.Type.Grass;
                    break;
                case MagicType.Fire:
                    bulletScript.type = Bullet.Type.Fire;
                    break;
                case MagicType.Water:
                    bulletScript.type = Bullet.Type.Water;
                    break;
                case MagicType.Electric:
                    bulletScript.type = Bullet.Type.Electric;
                    break;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = mousePos - new Vector2(transform.position.x, transform.position.y);

            bulletRigid.velocity = dir.normalized * bulletVel;
        }
        else
        {
            isShootMagic = false;
        }
    }

    public void ShootBow()
    {
        counterDelayShootBow += Time.deltaTime;
        if (startComboTimer)
        {
            comboTimer += Time.deltaTime;
        }
        if (comboTimer > maxComboTimer)
        {
            comboCount = 0;
            comboTimer = 0;
            startComboTimer = false;
        }

        Vector2 bowPosition = bow.transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePosition - bowPosition;
        
        if (facingRight)
        {
            bow.transform.right = direction;
        }
        else
        {
            bow.transform.right = -direction;
        }

        if (shootBowDown && counterDelayShootBow > delayShootBow)
        {
            StartCoroutine(ShootBowCombo(direction));
        }
    }

    IEnumerator ShootBowCombo(Vector2 direction)
    {
        counterDelayShootBow = 0.0f;
        startComboTimer = true;
        comboTimer = 0;

        for (int i = 0; i < comboCount + 1; i++)
        {
            GameObject newArrow = Instantiate(arrow, shotPoint.position, shotPoint.rotation);
            //newArrow.GetComponent<Rigidbody2D>().velocity = transform.right * arrowForce;
            newArrow.GetComponent<Rigidbody2D>().velocity = direction.normalized * arrowForce;

            yield return new WaitForSeconds(0.1f);
        }

        comboCount++;
        comboCount = comboCount % maxComboCount;
    }

    void Checks()
    {
        if (health <= 0)
        {
            transform.position = SpawnPoint;
            health = 100;
        }
    }

    void Melee()
    {
        if (Input.GetKey("e") && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("DoAttack1");
        }

        if (Input.GetKey("q") && !isAttacking)
        {
            animator.SetTrigger("DoCharge1");
            isAttacking = true;
        }
    }

    public void Finish_Anim()
    {
        isAttacking = false;
        isChargingAttack = false;
    }
    #endregion

    #region Events
    public void OnLanding()
    {
        isJump = false;
        jumpsLeft = maxJumps;
        animator.SetBool("IsJumping", false);
    }

    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("IsCrouching", isCrouching);
    }

    public void OnDamage()
    {
        health -= 10;
        sprite.color = Color.red;
        isDamage = true;

        Invoke("OnDamageOut", timeDamage);
    }
    public void OnDamageOut()
    {
        isDamage = false;
        sprite.color = Color.white;
    }
    #endregion

    #region Checks
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && !isDamage)
        {
            OnDamage();
        }

        if (collision.gameObject.layer == 12)
        {
            health = -10;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            canDash = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4)
        {
            isSwim = true;
            rigid.gravityScale = gravityScale;
        }
        if (collision.gameObject.layer == 11 && !isDamage)
        {
            OnDamage();
        }

        if (collision.gameObject.tag == "Coin")
        {
            coins++;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "Heart")
        {
            health += 20;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "CheckPoint")
        {
            SpawnPoint = collision.transform.position;
            Destroy(collision.gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 11)
        {
            if (!isDamage)
            {
                OnDamage();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4)
        {
            isSwim = false;
            rigid.gravityScale = 0;
        }
    }
    #endregion
}
