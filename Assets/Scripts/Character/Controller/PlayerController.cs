using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    //Animator and rigidbody
    private Animator anim;
    private Rigidbody2D rb;


    //climb
    [SerializeField] private float climbSpeed;
    private bool isClimbing;
    //Move
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool FacingRight = true;

    //Ground
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float checkRadius;
    [SerializeField] private int extraJumps;
    [SerializeField] private int extraJumpValue;
    
    private bool isGrounded;

    //shoot value
    [SerializeField] private Transform gunTip;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float fireRate;
    [SerializeField] private float nextFire;
    

    //attack
    [SerializeField] private float startTimeBtwAttack;
    [SerializeField] private Transform attackPos;
    [SerializeField] private LayerMask whatIsEnemies;
    [SerializeField] private float attackRange;
    [SerializeField] private int damage;

    private float timeBtwAttack;

    //Mana
    [SerializeField] private PlayerMana playerMana;
    [SerializeField] private PlayerHealth playerHealth;

    //check point
    private Vector3 respawnPoint;

   //booster
    bool booster = false;
    //combo attack
    float lastAttackTime = 0;
    int comboCount = 0;
    //invincible
    bool invincible = false;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        PlayerMana playerMana = GetComponent<PlayerMana>();
        transform.position = GameObject.FindGameObjectWithTag("StartPosition").transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            invincible = true;
            Invoke("TurnOffinvincible", 5f);
            if (invincible)
            {
                playerHealth.TakeDamage(0);
            }
            else
            {
                playerHealth.TakeDamage(10);
                //load scene
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (Time.time - lastAttackTime < 0.5f)
            {
                comboCount++;
                AttackClick();
                Debug.Log(comboCount);
            }
            else
            {
                comboCount = 0;
                AttackClick();
            }
            lastAttackTime = Time.time;
        }

        if (isClimbing)
        {
            rb.velocity = new Vector2(0, Time.deltaTime * climbSpeed);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            booster = true;
            damage *= 2;
            Invoke("TurnOffBooster", 5f);
            
        }

        DoubleJump();
        Move();
        FireBallClick();
       
        
    }
   
    void Flip()
    {
        FacingRight = !FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void Move(){
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        float move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(move * speed, rb.velocity.y);   
        if (move > 0 && !FacingRight)
        {   
            Flip();
        }
        else if (move < 0 && FacingRight)
        {      
               
            Flip();
        }
        //else if move =0 stop play sound
        anim.SetFloat("Speed", Mathf.Abs(move));
    }
    void DoubleJump(){
        if (isGrounded == true)
        {
            extraJumps = extraJumpValue;         
        }
        ///-----------------------------------------------------------------------------------------------------///
        if(Input.GetKeyDown(KeyCode.Space) && extraJumps >0)
        {
            rb.velocity = Vector2.up * jumpForce;
            extraJumps--;
            anim.SetTrigger("Jump");
        }
        else if(Input.GetKeyDown(KeyCode.Space) && extraJumps == 0 && isGrounded == true)
        {
            rb.velocity = Vector2.up * jumpForce;
            anim.SetTrigger("Jump");
        }  
    }
    void FireBallClick()
    {
        if (Input.GetKeyDown(KeyCode.Q) && playerMana.currentMana >= 10)
        {
            playerMana.UseMana(10);
            FireBall();
            anim.SetTrigger("Skill");
        }
        else
        {
            //if current mana < 10 player can't use firebullet
            if (Input.GetKeyDown(KeyCode.Q) && playerMana.currentMana < 10)
            {
                anim.SetTrigger("Jump");
            }
        }
    }
    void FireBall()
    {
        if(Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            if (FacingRight)
            {
                Instantiate(bullet, gunTip.position, Quaternion.Euler(new Vector3(0, 0, 0)));
                anim.SetTrigger("Skill");      
            }
            else
            {
                Instantiate(bullet, gunTip.position, Quaternion.Euler(new Vector3(0, 0, 180f)));
                anim.SetTrigger("Skill");
            }
                    
        }
    }     
    
    void AttackClick()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            anim.SetTrigger("Attack");
            Attack();
        }
    }
    void Attack()
    {
        if(timeBtwAttack<=0)
        {
            Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
            for(int i = 0 ; i < enemiesToDamage.Length;i++)
            {
                //check null
                if(enemiesToDamage[i].GetComponent<EnemyHealth>() != null)
                {
                    enemiesToDamage[i].GetComponent<EnemyHealth>().TakeDamage(damage);
                }
            }
                timeBtwAttack = startTimeBtwAttack;
        }
            else
            {
                timeBtwAttack -= Time.deltaTime;
            }

    }

    void OnTriggerEnter2D(Collider2D other)
    {       
        if(other.tag == "Shootable")
        {
            if(playerHealth.currentHealth > 0)
            {
                return;
            }
            else
            {
                //add heal = max heal
                playerHealth.Health();
                //add mana = max mana
                playerMana.Mana();
                transform.position = respawnPoint;
                //load scene
            }
        }
        else if(other.tag == "Checkpoint")
        {
            respawnPoint = other.transform.position;
            
        }  

        if(other.tag == "DeadZone") 
        {
            transform.position = respawnPoint;
            playerHealth.Health();
            playerMana.Mana();
            //load scene
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if(other.tag == "Finish")
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall" && Input.GetKey(KeyCode.R))
        {
            isClimbing = true;
        }
        else
        {
            isClimbing = false;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            isClimbing = false;
        }
    }
    

    void TurnOffBooster()
    {
        booster = false;
        damage /= 2;
        Debug.Log(damage);
    }
    
    void TurnOffinvincible()
    {
        invincible = false;
        Debug.Log("not invincible");
    }
}
